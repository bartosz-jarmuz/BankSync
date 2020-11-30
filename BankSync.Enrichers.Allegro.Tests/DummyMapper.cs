
using BankSync.Model;

namespace BankSync.Enrichers.Allegro.Tests
{
    internal class DummyMapper : IDataMapper
    {
        public string Map(string input)
        {
            return input;
        }
    }
}