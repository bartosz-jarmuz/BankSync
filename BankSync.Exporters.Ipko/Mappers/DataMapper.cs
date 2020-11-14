// -----------------------------------------------------------------------
//  <copyright file="DataMapper.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BankSync.Exporters.Ipko.Mappers
{
    public class ConfigurableDataMapper : IDataMapper
    {
        public ConfigurableDataMapper(FileInfo mappingFile)
        {
            this.LoadNodes(mappingFile);
        }

        private void LoadNodes(FileInfo mappingFile)
        {
            if (mappingFile.Exists)
            {
                var xDoc = XDocument.Load(mappingFile.FullName);
                this.Nodes = xDoc.Root?.Descendants("From")?.ToList();
            }
        }

        private List<XElement> Nodes { get; set; }

        public string Map(string input)
        {
            if (input == null)
            {
                return null;
            }
            var mapped = this.Nodes.FirstOrDefault(x =>  x.Value.Trim().Equals(input.Trim(), StringComparison.OrdinalIgnoreCase));
            if (mapped != null)
            {
                return mapped?.Parent?.Attribute("To")?.Value??input;
            }
            return input;
        }


        
    }
}