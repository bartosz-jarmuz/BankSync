using System;
using System.Collections.Generic;
using System.Linq;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Logging;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Enrichers.Allegro
{
    internal class RefundEnricher
    {

        private readonly IBankSyncLogger logger;
        private readonly BankSyncConverter converter;

        public RefundEnricher(IBankSyncLogger logger)
        {
            this.logger = logger;
            this.converter = new BankSyncConverter();
        }

        public void EnrichAllegroEntry(BankEntry entry, List<AllegroDataContainer> allData, List<BankEntry> updatedEntries, out decimal discount)
        {
            discount = 0;
            List<Myorder> relevantOrders = this.GetRelevantOrders(entry, allData, out AllegroDataContainer container,
                out List<Offer> potentiallyRelatedOfferIds);
            if (relevantOrders != null && relevantOrders.Any())
            {
                foreach (Myorder order in relevantOrders)
                {
                    if (entry.Amount == this.converter.ToDecimal(order.payment.amount.amount))
                    {
                        for (int offerIndex = 0; offerIndex < order.offers.Length; offerIndex++)
                        {
                            BankEntry newEntry = PrepareNewBankEntryForOffer(order, offerIndex, entry, container);

                            updatedEntries.Add(newEntry);
                        }
                    }
                    else
                    {
                        HandlePartialRefunds(entry, updatedEntries, order, container);
                    }
                    
                    
                }
            } 
            else
            {
                //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                EnrichUnrecognizedAllegroOffer(entry, container, potentiallyRelatedOfferIds);
                updatedEntries.Add(entry);
            }
        }

        private void HandlePartialRefunds(BankEntry entry, List<BankEntry> updatedEntries, Myorder order,
            AllegroDataContainer container)
        {
            List<Offer> offersWithProperPrice =
                order.offers.Where(x => this.converter.ToDecimal(x.offerPrice.amount) == entry.Amount).ToList();

            if (offersWithProperPrice.Any())
            {
                if (offersWithProperPrice.Count == 1)
                {
                    Offer offer = offersWithProperPrice.First();
                    BankEntry newEntry = BankEntry.Clone(entry);

                    newEntry.Note = $"ZWROT: {offer.title} (Ilość sztuk: {offer.quantity}, Oferta {offer.id})";

                    newEntry.Payer = "allegro.pl - " + order.seller.login;
                    newEntry.Recipient = container.ServiceUserName;
                    newEntry.FullDetails += $"\r\nPłatność Allegro: {order.payment.id}.\r\n URL: {offer.friendlyUrl}";
                    updatedEntries.Add(newEntry);
                }
                else
                {
                    BankEntry newEntry = BankEntry.Clone(entry);

                    newEntry.Note = "ZWROT pasujący do jednej z ofert: " +
                                    string.Join("\r\n", offersWithProperPrice.Select(x => x.title));

                    newEntry.Payer = "allegro.pl - " + order.seller.login;
                    newEntry.Recipient = container.ServiceUserName;
                    newEntry.FullDetails += $"\r\nPłatność Allegro: {order.payment.id}.\r\n{newEntry.Note}";
                    updatedEntries.Add(newEntry);
                }
            }
        }

        private BankEntry PrepareNewBankEntryForOffer(Myorder allegroEntry, int offerIndex, BankEntry entry,
            AllegroDataContainer container)
        {
            Offer offer = allegroEntry.offers[offerIndex];
            BankEntry newEntry = BankEntry.Clone(entry);
            
            newEntry.Amount = this.converter.ToDecimal(offer.offerPrice.amount);

            newEntry.Note = $"ZWROT: {offer.title} (Ilość sztuk: {offer.quantity}, Oferta {offer.id}, Pozycja {offerIndex + 1}/{allegroEntry.offers.Length})";
           
            newEntry.Payer = "allegro.pl - " + allegroEntry.seller.login;
            newEntry.Recipient = container.ServiceUserName;
            newEntry.FullDetails += $"\r\nPłatność Allegro: {allegroEntry.payment.id}.\r\n URL: {offer.friendlyUrl}";
            return newEntry;
        }

        
        private List<Myorder> GetRelevantOrders(BankEntry entry, List<AllegroDataContainer> allegroDataContainers, out AllegroDataContainer associatedContainer, out List<Offer> potentiallyRelatedOfferIds)
        {
            associatedContainer = allegroDataContainers.FirstOrDefault(x => x.ServiceUserName == entry.Payer);
            AllegroData model = associatedContainer?.Model;
            List<Offer> offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds = null;

            List<Myorder> result = null;
            if (model != null)
            {
                result = this.GetAllegroEntries(entry, model, out _ );
            }

            if (result != null)
            {
                potentiallyRelatedOfferIds = null;
                return result;
            }
            else
            {
                //the payer can be empty or it can be somehow incorrect, but if we have an entry that matches the exact price and date... it's probably IT
                foreach (AllegroDataContainer container in allegroDataContainers)
                {
                    List<Myorder> entries = this.GetAllegroEntries(entry, container.Model, out List<Offer> offersMatchingPrice );
                    if (offersMatchingPrice != null && offersMatchingPrice.Any())
                    {
                        offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds = new List<Offer>(offersMatchingPrice);
                    }
                    if (entries != null && entries.Any())
                    {
                        associatedContainer = container;
                        potentiallyRelatedOfferIds = null;
                        return entries;
                    }
                }
            }

            potentiallyRelatedOfferIds = offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds?.ToList(); 
            return null;
        }

        
        private List<Myorder> FindOrdersWhichMatchEntryPriceFullyOrPartially(BankEntry entry, AllegroData model)
        {
            //first try finding the orders which fully correspond to the price
            List<Myorder> allegroOrders = model.myorders.myorders
                .Where(x => x.payment.startDate.Date <= entry.Date)
                .Where(x =>
                    this.converter.ToDecimal(x.payment.amount.amount) == this.converter.ToDecimal(entry.Amount.ToString().Trim('-'))
                ).ToList();

            
            if (allegroOrders.Count == 0)
            {
                //and if that doesnt succeed, find orders where at least one offer matches the price
                allegroOrders = model.myorders.myorders
                    //  .Where(x=>x.payment.startDate < entry.Date)
                    .Where(x =>
                        x.offers.Any(x =>
                            this.converter.ToDecimal(x.offerPrice.amount) == this.converter.ToDecimal(entry.Amount.ToString().Trim('-')))
                    ).ToList();
            }

            return allegroOrders;
        }
        
        private List<Myorder> GetAllegroEntries(BankEntry entry, AllegroData model, out List<Offer> offersMatchingPrice )
        {
            offersMatchingPrice = null;
            List<Myorder> allegroOrders = FindOrdersWhichMatchEntryPriceFullyOrPartially(entry, model);

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
                List<Myorder> dateFilteredEntries = allegroOrders.Where(x => x.payment.endDate.Date == entry.Date.Date).ToList();
                if (dateFilteredEntries.Count < 1)
                { 
                    return FindRefundEntries(entry, ref offersMatchingPrice, allegroOrders);
                }
                else
                {
                    return FindProperEntriesBasedOnDateTime(entry, dateFilteredEntries);
                }
            }
        }

        private static List<Myorder> FindProperEntriesBasedOnDateTime(BankEntry entry, List<Myorder> dateFilteredEntries)
        {
            string paymentId = dateFilteredEntries.First().payment.id;
            if (dateFilteredEntries.All(x => paymentId == x.payment.id))
            {
                //that's all the orders from the same payment, so it's one big purchase
                return dateFilteredEntries;
            }
            else
            {
                //we have multiple purchases for the same amount on the same date - lets try finding matching timestamp (if timestamp is available)
                DateTime entryTimeUtc = entry.Date.ToUniversalTime();
                List<Myorder> timeMatchingEntries = dateFilteredEntries
                    .Where(x => x.payment.endDate > entryTimeUtc && x.payment.startDate < entryTimeUtc).ToList();

                if (timeMatchingEntries.Count == 1)
                {
                    return timeMatchingEntries;
                }
                else
                {
                    var orderedByTime = timeMatchingEntries.OrderBy(x => x.payment.endDate);
                    return orderedByTime.Take(1).ToList();
                }
            }
        }
        private List<Myorder> FindRefundEntries(BankEntry entry, ref List<Offer> offersMatchingPrice, List<Myorder> allegroEntries)
        {
            List<Myorder> dateFilteredEntries;
            //if it's a refund, then we have one more chance of finding the right one
            //the refund must have happened AFTER the purchase
            dateFilteredEntries = allegroEntries.Where(x => x.payment.endDate.Date < entry.Date).ToList();

            if (dateFilteredEntries.Count != 1)
            {
                //so now we have a potentially long list of entries purchased at the same price (e.g. 9.99),
                //so we cannot figure out which one was actually refunded.
                //too bad there is no refund note or reference
                offersMatchingPrice = dateFilteredEntries
                    .SelectMany(x => x.offers.Where(x =>
                        this.converter.ToDecimal(x.offerPrice.amount) == this.converter.ToDecimal(entry.Amount.ToString().Trim('-')))
                    ).ToList();

                return null;
            }
            else
            {
                return dateFilteredEntries;
            }
        }
        
        private static void EnrichUnrecognizedAllegroOffer(BankEntry entry, AllegroDataContainer container, List<Offer> potentiallyRelatedOfferIds)
        {
            entry.Payer = "allegro.pl (Nierozpoznany sprzedawca)";
            if (!string.IsNullOrEmpty(container?.ServiceUserName))
            {
                entry.Recipient = container.ServiceUserName;
            }
            else
            {
                entry.Recipient = "Nierozpoznany odbiorca";
            }
            if (potentiallyRelatedOfferIds != null && potentiallyRelatedOfferIds.Any())
            {
                entry.Note = "ZWROT pasujący do jednej z ofert: " + string.Join("\r\n", potentiallyRelatedOfferIds.Select(x=>x.title));
            }
            else
            {
                entry.Note = "Nierozpoznany zwrot";
            }
        }

    }
}