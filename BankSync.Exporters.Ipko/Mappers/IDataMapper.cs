// -----------------------------------------------------------------------
//  <copyright file="IDataMapper.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Exporters.Ipko.Mappers
{
    public interface IDataMapper
    {
        string Map(string input);
    }
}