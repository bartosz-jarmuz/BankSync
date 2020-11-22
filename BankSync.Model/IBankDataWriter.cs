// -----------------------------------------------------------------------
//  <copyright file="IDataWriter.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace BankSync.Model
{
    public interface IBankDataWriter
    {
        public Task Write(BankDataSheet data);
    }
}