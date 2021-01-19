// -----------------------------------------------------------------------
//  <copyright file="DataEnricherExecutor.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Analyzers.AI;
using BankSync.Analyzers.InternalTransactions;
using BankSync.Config;
using BankSync.Enrichers.Allegro;
using BankSync.Logging;
using BankSync.Model;

namespace BankSyncRunner
{
    public class DataAnalyzersExecutor
    {
        public DataAnalyzersExecutor(IBankSyncLogger logger, DirectoryInfo workingDir)
        {
            allIfsAnalyzer = new AllIfsAnalyzer(new FileInfo(Path.Combine(workingDir.FullName, "Tags.xml")), logger);
            internalTransfersAnalyzer = new InternalTransactionsAnalyzer(new FileInfo(Path.Combine(workingDir.FullName,"InternalTransactions.xml")));
        }

        private IBankDataAnalyzer allIfsAnalyzer;
        private IBankDataAnalyzer internalTransfersAnalyzer;

        private List<IBankDataAnalyzer> Enrichers => new List<IBankDataAnalyzer>()
            {this.allIfsAnalyzer, this.internalTransfersAnalyzer};

        public void AnalyzeData(BankDataSheet data)
        {
            foreach (IBankDataAnalyzer bankDataEnricher in this.Enrichers)
            {
                bankDataEnricher.AssignCategories(data);
            }
        }
    }
}