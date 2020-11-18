using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using BankSync.Model;
using Newtonsoft.Json;

namespace BankSync.Writers.Json
{
    public class JsonBankDataWriter : IBankDataWriter
    {
        private readonly string targetFilePath;

        public JsonBankDataWriter(string targetFilePath)
        {
            this.targetFilePath = targetFilePath;
        }

        public void Write(WalletDataSheet data)
        {
            var map = new TagMap();

            foreach (WalletEntry walletEntry in data.Entries)
            {
                map.Values.Add(new KeyValuePair<int, List<string>>(walletEntry.OriginalBankEntryId, walletEntry.Tags));
            }


            data.TagMap = map;

            string serialized = JsonConvert.SerializeObject(data);

            File.WriteAllText(this.targetFilePath, serialized);
        }
    }
}
