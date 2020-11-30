using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BankSync.Enrichers.Allegro.Tests
{
    internal class FakeDataLoader : IAllegroDataLoader
    {
        private readonly string filePath;

        public FakeDataLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public Task<List<AllegroDataContainer>> LoadAllData(DateTime oldestEntry)
        {
            var content = File.ReadAllText(this.filePath);
            var model = JsonConvert.DeserializeObject<AllegroDataContainer>(content);
            return Task.FromResult(new List<AllegroDataContainer>(){model});

        }
    }
}