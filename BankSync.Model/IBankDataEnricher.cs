// -----------------------------------------------------------------------
//  <copyright file="IBankDataEnricher.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace BankSync.Model
{
    public interface IBankDataEnricher
    {
        public Task Enrich(WalletDataSheet data);
    }
}