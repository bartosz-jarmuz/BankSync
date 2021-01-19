using System;
using System.Collections.Generic;
using System.Linq;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Logging;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Enrichers.Allegro
{
    internal class PurchaseEnricher
    {
        private readonly IBankSyncLogger logger;

        public PurchaseEnricher(IBankSyncLogger logger)
        {
            this.logger = logger;
        }

        public void EnrichAllegroEntry(BankEntry entry, List<AllegroDataContainer> allData, List<BankEntry> updatedEntries, out decimal buyerPaidAmount)
        {
            buyerPaidAmount = 0;
            List<Myorder> relevantOrders = this.GetRelevantOrders(entry, allData, out AllegroDataContainer container);
            
            
            
            if (relevantOrders != null && relevantOrders.Any())
            {
                buyerPaidAmount = this.CalculateTotalAmount(relevantOrders);
                //multiple orders can be covered by a single payment
                foreach (Myorder order in relevantOrders)
                {
                    for (int offerIndex = 0; offerIndex < order.offers.Length; offerIndex++)
                    {
                        BankEntry newEntry = PrepareNewBankEntryForOffer(order, offerIndex, entry, container);
                        updatedEntries.Add(newEntry);
                    }
                    AddDeliveryCost(entry, updatedEntries, order, container);
                }
            }
            else
            {
                //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                EnrichUnrecognizedAllegroOffer(entry);
                updatedEntries.Add(entry);
            }
        }

        private decimal CalculateTotalAmount(List<Myorder> relevantOrders)
        {
            Myorder firstOrder = relevantOrders.First();
            Payment payment = firstOrder.payment;

            if (relevantOrders.All(x => x.payment.id == payment.id))
            {
                return BankSyncConverter.ToDecimal(payment.buyerPaidAmount.amount) *-1;
            }
            else
            {
                IEnumerable<IGrouping<string, Myorder>> groupings = relevantOrders.GroupBy(x => x.payment.id);
                decimal totalAmount = 0;
                foreach (IGrouping<string, Myorder> grouping in groupings)
                {
                   Payment groupPayment = grouping.First().payment;
                    decimal partial =   BankSyncConverter.ToDecimal(groupPayment.buyerPaidAmount.amount);
                    totalAmount += partial;
                }

                return totalAmount*-1;
            }
        }

        private static BankEntry PrepareNewBankEntryForOffer(Myorder allegroEntry, int offerIndex, BankEntry entry, AllegroDataContainer container)
        {
            Offer offer = allegroEntry.offers[offerIndex];
            BankEntry newEntry = BankEntry.Clone(entry);
            newEntry.Amount = BankSyncConverter.ToDecimal(offer.offerPrice.amount) * -1;

            newEntry.Note = $"{offer.title} (Ilość sztuk: {offer.quantity}, Oferta {offer.id}, Pozycja {offerIndex + 1}/{allegroEntry.offers.Length})";
            newEntry.Recipient = "allegro.pl - " + allegroEntry.seller.login;
            if (string.IsNullOrEmpty(newEntry.Payer))
            {
                newEntry.Payer = container.ServiceUserName;
            }
            newEntry.FullDetails += $"\r\nPłatność Allegro: {allegroEntry.payment.id}.\r\n URL: {offer.friendlyUrl}";
            return newEntry;
        }
        
        
        private List<Myorder> GetRelevantOrders(BankEntry entry, List<AllegroDataContainer> allegroDataContainers, out AllegroDataContainer associatedContainer)
        {
            //get data model for the payer
            associatedContainer = allegroDataContainers.FirstOrDefault(x => x.ServiceUserName == entry.Payer);
            string associatedUserName = associatedContainer?.ServiceUserName;
            AllegroData model = associatedContainer?.Model;
            
            List<Myorder> result = null;
            if (model != null)
            {
                result = this.GetAllegroOrders(entry, model);
                
            }
            //model might be not-null but the result might be null
            if (result != null)
            {
                return result;
            }
            else
            {
                //the payer can be empty or it can be somehow incorrect, but if we have an order that matches the exact price and date... it's probably IT
                //so try finding order in any persons data
                List<AllegroDataContainer> alternativeContainers = FindAlternativeDataContainers(allegroDataContainers, associatedUserName);

                foreach (AllegroDataContainer container in alternativeContainers)
                {
                    List<Myorder> allegroOrders = this.GetAllegroOrders(entry, container.Model);
                    if (allegroOrders != null && allegroOrders.Any())
                    {
                        associatedContainer = container;
                        return allegroOrders;
                    }
                }
            }
            return null;
        }

        private static List<AllegroDataContainer> FindAlternativeDataContainers(List<AllegroDataContainer> allegroDataContainers, string associatedUserName)
        {
            List<AllegroDataContainer> alternativeContainers;
            if (associatedUserName != null)
            {
                //don't check the container of the same user again
                alternativeContainers = allegroDataContainers
                    .Where(x => x.ServiceUserName != associatedUserName).ToList();
            }
            else
            {
                alternativeContainers = allegroDataContainers.ToList();
            }

            return alternativeContainers;
        }

        private static void AddDeliveryCost(BankEntry entry, List<BankEntry> updatedEntries, Myorder allegroEntry,
            AllegroDataContainer container)
        {
            if (allegroEntry.delivery.cost.amount != "0.00")
            {
                //lets add a delivery cost as a separate entry, but assign it a category etc. of the most expensive item
                Offer mostExpensive =
                    allegroEntry.offers.OrderByDescending(x => BankSyncConverter.ToDecimal(x.offerPrice.amount)).First();

                BankEntry deliveryEntry = BankEntry.Clone(entry);
                deliveryEntry.Amount = BankSyncConverter.ToDecimal(allegroEntry.delivery.cost.amount) * -1;
                deliveryEntry.Note =
                    $"DOSTAWA: {mostExpensive.title} (Oferta {mostExpensive.id}, Suma zamówień: {allegroEntry.offers.Length})";
                deliveryEntry.Recipient = "allegro.pl - " + allegroEntry.seller.login;
                deliveryEntry.Tags.Add("dostawa");
                if (string.IsNullOrEmpty(deliveryEntry.Payer))
                {
                    deliveryEntry.Payer = container.ServiceUserName;
                }

                deliveryEntry.FullDetails +=
                    $"\r\nPłatność Allegro: {allegroEntry.payment.id}.\r\n URL: {mostExpensive.friendlyUrl}";
                updatedEntries.Add(deliveryEntry);
            }
        }

        
        private static List<Myorder> FindProperOrdersBasedOnDateTime(BankEntry entry, List<Myorder> dateFilteredOrders)
        {
            string paymentId = dateFilteredOrders.First().payment.id;
            if (dateFilteredOrders.All(x => paymentId == x.payment.id))
            {
                //that's all the orders from the same payment, so it's a one big purchase
                return dateFilteredOrders;
            }
            else
            {
                //we have multiple purchases for the same amount on the same date - lets try finding matching timestamp (if timestamp is available)
                DateTime entryTimeUtc = entry.Date.ToUniversalTime();
                List<Myorder> timeMatchingEntries = dateFilteredOrders
                    .Where(x => x.payment.endDate > entryTimeUtc && x.payment.startDate < entryTimeUtc).ToList();

                if (timeMatchingEntries.Count == 1)
                {
                    return timeMatchingEntries;
                }
                else
                {
                    IOrderedEnumerable<Myorder> orderedByTime = timeMatchingEntries.OrderBy(x => x.payment.endDate);
                    return orderedByTime.Take(1).ToList();
                }
            }
        }
        
        private List<Myorder> GetAllegroOrders(BankEntry entry, AllegroData model)
        {
            //first try finding the orders which fully correspond to the price and more or less the date
            List<Myorder> allegroOrders = model.parameters.myorders.myorders
                .Where(x => x.orderDate.Date <= entry.Date.Date)
                .Where(x => ( entry.Date.Date - x.orderDate.Date).TotalDays < 30)
                .Where(x =>
                    BankSyncConverter.ToDecimal(x.payment.buyerPaidAmount.amount) == BankSyncConverter.ToDecimal(entry.Amount.ToString().Trim('-'))
                ).ToList();


            if (allegroOrders.Count == 0)
            {
                return null;
            }
            if (allegroOrders.Count == 1)
            {
                return allegroOrders;
            }
            else
            {
                List<Myorder> dateFilteredOrders = allegroOrders.Where(x => x.payment.endDate.Date == entry.Date.Date).ToList();
                if (dateFilteredOrders.Count < 1)
                {
                    this.logger.Warning($"Unrecognized entry. {allegroOrders.Count} orders matched the price, but none matched the payment date. {entry}");
                    return null;
                }
                else
                {
                    return FindProperOrdersBasedOnDateTime(entry, dateFilteredOrders);
                }
            }
        }

        private static void EnrichUnrecognizedAllegroOffer(BankEntry entry)
        {
            if (entry.Note == entry.FullDetails)
            {
                entry.Note = AllegroBankDataEnricher.NierozpoznanyZakup;
            }
            else
            {
                entry.Note = AllegroBankDataEnricher.NierozpoznanyZakup + " - " + entry.Note;
            }
        }

        
    }
}