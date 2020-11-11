// -----------------------------------------------------------------------
//  <copyright file="IBankDataAnalyzer.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;

namespace BankSync.Model
{
    public interface IBankDataAnalyzer
    {
        public void AddTags(WalletDataSheet data);
    }
}