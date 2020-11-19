// -----------------------------------------------------------------------
//  <copyright file="IDataWriter.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Model
{
    public interface IBankDataWriter
    {
        public void Write(BankDataSheet data);
    }
}