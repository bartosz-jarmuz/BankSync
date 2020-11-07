// -----------------------------------------------------------------------
//  <copyright file="NoActionDataMapper.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Exporters.Ipko.Mappers
{
    public class NoActionDataMapper : IDataMapper
    {
        public string Map(string input)
        {
            return input;
        }

        public string MapFromAccount(string accountData)
        {
            return accountData;
        }
    }
}