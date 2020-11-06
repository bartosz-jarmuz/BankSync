// -----------------------------------------------------------------------
//  <copyright file="IDataWriter.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Model
{
    public interface IBankDataWriter
    {
        public void Write(WalletDataSheet data);
    }
}