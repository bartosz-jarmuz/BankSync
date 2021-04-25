// -----------------------------------------------------------------------
//  <copyright file="IBankDataEnricher.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace BankSync.Model
{
    public interface IBankDataEnricher
    {
        public Task Enrich(BankDataSheet data, DateTime startTime, DateTime endTime,
            Action<BankDataSheet> completionCallback);
    }
}