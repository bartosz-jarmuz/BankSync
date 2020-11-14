using System;
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
            string serialized = JsonConvert.SerializeObject(data);

            File.WriteAllText(this.targetFilePath, serialized);
        }
    }
}
