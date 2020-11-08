// -----------------------------------------------------------------------
//  <copyright file="IDataMapper.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Exporters.Ipko.Mappers
{
    public interface IDataMapper
    {
        string Map(string input);
    }
}