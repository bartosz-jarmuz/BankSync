using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

        public Task Write(BankDataSheet data)
        {
            string serialized = JsonConvert.SerializeObject(data);

            File.WriteAllText(this.targetFilePath, serialized);
            
            return Task.CompletedTask;

        }
    }
}
