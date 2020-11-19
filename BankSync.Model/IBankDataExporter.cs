// -----------------------------------------------------------------------
//  <copyright file="IBankDataExporter.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace BankSync.Model
{
    public interface IBankDataExporter
    {
        public Task<BankDataSheet> GetData(DateTime startTime, DateTime endTime);
    }
}