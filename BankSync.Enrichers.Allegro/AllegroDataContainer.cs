// -----------------------------------------------------------------------
//  <copyright file="AllegroDataContainer.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using BankSync.Enrichers.Allegro.Model;

namespace BankSync.Enrichers.Allegro
{
    internal class AllegroDataContainer
    {
        public AllegroData Model { get; internal set; }
        public string ServiceUserName { get; internal set; }

        public AllegroDataContainer(AllegroData model, string serviceUserName)
        {
            this.Model = model;
            this.ServiceUserName = serviceUserName;
        }
    }
}