using System;
using System.Collections.Generic;
using System.Text;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Utilities;

namespace BankSync.Enrichers.Allegro
{
    public static class ModelExtensions
    {
        public static decimal GetAmount(this Myorder order, BankSyncConverter converter)
        {
            if (order.payment.amount?.amount != null)
            {
                return converter.ToDecimal(order.payment.amount.amount);
            }
            else
            {
                return converter.ToDecimal(order.totalCost.amount);
            }
        }
    }
}
