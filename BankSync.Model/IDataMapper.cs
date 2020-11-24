// -----------------------------------------------------------------------
//  <copyright file="IDataMapper.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Model
{
    public interface IDataMapper
    {
        string Map(string input);
    }
}