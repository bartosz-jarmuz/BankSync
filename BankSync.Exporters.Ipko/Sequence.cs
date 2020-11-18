// -----------------------------------------------------------------------
//  <copyright file="Sequence.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Exporters.Ipko
{
    internal class Sequence
    {
        private int value;
        public int GetValue()
        {
            int toReturn = this.value;
            this.value++;
            return toReturn;
        }
    }
}