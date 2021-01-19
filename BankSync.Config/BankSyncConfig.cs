// -----------------------------------------------------------------------
//  <copyright file="Config.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Config
{
    public class BankSyncConfig
    {
        private readonly XDocument configXDoc;

        public BankSyncConfig(FileInfo configFile, Func<string, string> provideInput)
        {
            this.ConfigFilePath = configFile.FullName;
            this.configXDoc = XDocument.Load(configFile.FullName);
            this.LoadServices(provideInput, () => this.configXDoc.Save(configFile.FullName));
        }

        public string ConfigFilePath { get;  }

        public List<ServiceConfig> Services { get; } = new List<ServiceConfig>();

        public void LoadServices(Func<string, string> provideInput, Action updateConfig)
        {
            foreach (XElement service in this.configXDoc.Root.Elements("Service"))
            {
                this.Services.Add(new ServiceConfig(this,service, provideInput, updateConfig));
            }
        }

    }

    public class ServiceConfig
    {
        public readonly BankSyncConfig Config;
        public XElement ServiceElement { get; }

        public ServiceConfig(BankSyncConfig config, XElement serviceElement, Func<string, string> provideInput,
            Action updateConfig)
        {
            this.Config = config;
            this.ServiceElement = serviceElement;
            this.Name = serviceElement.Attribute("Name").Value;
            foreach (XElement userElement in serviceElement.Elements("User"))
            {
                this.Users.Add(new ServiceUser(this, userElement, provideInput, updateConfig));
            }
        }

        public string Name { get; set; }

        public List<ServiceUser> Users { get; set; } = new List<ServiceUser>();
    }

    public class ServiceUser
    {
        public XElement UserElement { get; }
        public  readonly ServiceConfig ServiceConfig;
        public string UserName { get; }

        public ServiceUser(ServiceConfig serviceConfig, XElement userElement, Func<string, string> provideInput, Action updateConfig)
        {
            this.UserElement = userElement;
            this.ServiceConfig = serviceConfig;
            this.UserName = userElement.Attribute("Name")?.Value ?? "Name not specified";
            XElement accounts = userElement.Element("Accounts");
            if (accounts != null)
            {
                foreach (XElement accountEle in accounts.Elements("Account"))
                {
                    Account account = Account.CreateInstance(accountEle);
                    if (account != null)
                    {
                        this.Accounts.Add(account);
                    }
                }
            }
            XElement cards = userElement.Element("Cards");
            if (cards != null)
            {
                foreach (XElement cardEle in cards.Elements("Card"))
                {
                    Card card = Card.CreateInstance(cardEle);
                    if (card != null)
                    {
                        this.Cards.Add(card);
                    }
                }
            }

            this.LoadCredentials(userElement, provideInput, updateConfig);
        }

        private void LoadCredentials(XElement userElement, Func<string, string> provideInput, Action updateConfig)
        {
            SecureString login = this.LoadStoredValue(userElement, "Login", provideInput, updateConfig);
            SecureString password = this.LoadStoredValue(userElement, "Password", provideInput, updateConfig);

            this.Credentials = new Credentials()
            {
                Id = login.ToInsecureString(),
                Password = password
            };
        }

        private SecureString LoadStoredValue(XElement userElement, string elementToBeLoadedName,
            Func<string, string> provideInput, Action updateConfig)
        {
            XElement elementToBeLoaded = userElement.Element(elementToBeLoadedName);
            if (elementToBeLoaded != null && !string.IsNullOrEmpty(elementToBeLoaded.Value))
            {
                return elementToBeLoaded.Value.DecryptString();
            }
            else
            {
                string newLogin = provideInput(
                    $"Provide '{this.ServiceConfig.Name}' {elementToBeLoadedName} for user '{this.UserName}' (WILL BE STORED ENCRYPTED)");
                userElement.Add(new XElement(elementToBeLoadedName, newLogin.ToSecureString().EncryptString()));
                updateConfig();
                return newLogin.ToSecureString();
            }
        }

        public Credentials Credentials { get; set; }

        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Card> Cards { get; set; } = new List<Card>();
    }

    public class Account
    {
        public static Account CreateInstance(XElement account)
        {
            string number = account?.Element("Number")?.Value;
            if (string.IsNullOrEmpty(number))
            {
                return null;
            }
            return new Account(number);
        }

        private Account(string number)
        {
            this.Number = number;
        }

        public string Number { get; set; }
    }

    public class Card
    {
        public static Card CreateInstance(XElement card)
        {
            string number = card?.Element("Number")?.Value;
            if (string.IsNullOrEmpty(number))
            {
                return null;
            }
            return new Card(number);
        }

        private Card(string number)
        {
            this.Number = number;
        }

        public string Number { get; set; }
    }


}