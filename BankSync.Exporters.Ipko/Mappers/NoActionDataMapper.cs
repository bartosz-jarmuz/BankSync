// -----------------------------------------------------------------------
//  <copyright file="NoActionDataMapper.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
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