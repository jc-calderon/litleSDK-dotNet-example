using System;
using System.Collections.Generic;
using System.Linq;
using Litle.Sdk;
using Litle.Sdk.Properties;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace LittleProjectSDK_Debug
{
    internal static class Program
    {
        private static LitleOnline litle = new LitleOnline(GetSettings());

        private static void Main(string[] args)
        {
            DoAuthorization(GetContacts().First(), GetCreditCards().First());
            //DoSale(GetContacts().First(), GetCreditCards().First());
            //DoAccountUpdater(GetCreditCards());
            //DoRFRRequest(DateTime.Today);

            Console.ReadLine();
        }

        #region Batch processing

        private static void DoAccountUpdater(List<cardType> creditCards)
        {
            Console.WriteLine("************************ Do Account Updater **************************");
            litleRequest litleRequestData = new litleRequest(GetSettings(SettingsType.Prelive));
            batchRequest batchRequestData = new batchRequest(GetSettings(SettingsType.Prelive));

            var orderId = 1;
            foreach (var creditCard in creditCards)
            {
                accountUpdate accountUpdateData = new accountUpdate
                {
                    orderId = orderId++.ToString(),
                    card = creditCard
                };

                batchRequestData.addAccountUpdate(accountUpdateData);
            }

            litleRequestData.addBatch(batchRequestData);

            var batchName = litleRequestData.sendToLitle();
            litleRequestData.blockAndWaitForResponse(batchName, EstimatedResponseTime(0, creditCards.Count));
            var litleResponse = litleRequestData.receiveFromLitle(batchName);

            ShowLitleBatchResponse(litleResponse);
        }

        private static void DoRFRRequest(DateTime date)
        {
            Console.WriteLine("************************ Do RFR Request **************************");
            var merchantId = GetSettings(SettingsType.Prelive)["merchantId"];

            var rfr = new RFRRequest();
            rfr.accountUpdateFileRequestData = new accountUpdateFileRequestData();
            rfr.accountUpdateFileRequestData.postDay = date;
            rfr.accountUpdateFileRequestData.merchantId = merchantId;

            var req = new litleRequest(GetSettings(SettingsType.Prelive));
            req.addRFRRequest(rfr);

            var rfrBatchName = req.sendToLitle();

            req.blockAndWaitForResponse(rfrBatchName, EstimatedResponseTime(0, 2));
            var litleRfrResponse = req.receiveFromLitle(rfrBatchName);

            ShowLitleBatchResponse(litleRfrResponse);
        }

        #endregion Batch processing

        #region Transactions

        private static void DoAuthorization(contact contact, cardType card)
        {
            authorization authorization = new authorization
            {
                orderId = "1",
                amount = 10010,
                orderSource = orderSourceType.ecommerce,
                billToAddress = contact,
                card = card,
                filtering = GetFilteringType()
            };

            authorizationResponse response = litle.Authorize(authorization);
            Console.WriteLine("************************ Do Authorization **************************");
            SerializeJson(response);
        }

        private static void DoSale(contact contact, cardType card)
        {
            sale sale = new sale
            {
                orderId = "1",
                amount = 10010,
                orderSource = orderSourceType.ecommerce,
                billToAddress = contact,
                card = card,
                filtering = GetFilteringType()
            };

            saleResponse response = litle.Sale(sale);
            Console.WriteLine("************************ Do Sale **************************");
            SerializeJson(response);
        }

        #endregion Transactions

        #region Utils

        private static List<contact> GetContacts()
        {
            var contacts = new List<contact>
            {
                new contact
                {
                    name = "John Smith",
                    addressLine1 = "1 Main St.",
                    city = "Burlington",
                    state = "MA",
                    zip = "01803",
                    country = countryTypeEnum.US
                },
                new contact
                {
                    name = "John Smith",
                    addressLine1 = "1 Main St.",
                    city = "Burlington",
                    state = "MA",
                    zip = "01803",
                    country = countryTypeEnum.US
                }
            };

            return contacts;
        }

        private static List<cardType> GetCreditCards()
        {
            var creditCards = new List<cardType>
            {
                new cardType
                {
                    type = methodOfPaymentTypeEnum.MC,
                    number = "5194560012341234",
                    expDate = "1250"
                },
                new cardType
                {
                    type = methodOfPaymentTypeEnum.MC,
                    number = "5435101234510196",
                    expDate = "0750"
                }
            };

            return creditCards;
        }

        private static int EstimatedResponseTime(int numAuthsAndSales, int numRest)
        {
            return (int)(5 * 60 * 1000 + 2.5 * 1000 + numAuthsAndSales * (1 / 5) * 1000 + numRest * (1 / 50) * 1000) * 5;
        }

        private static filteringType GetFilteringType()
        {
            var filteringType = new filteringType
            {
                prepaid = true
            };

            return filteringType;
        }

        private static Dictionary<string, string> GetSettings(SettingsType settingsType = SettingsType.Sandbox)
        {
            var settings = new Dictionary<string, string>
            {
                { "version", "9.3" },
                { "printxml", "true" },
                { "timeout", "30" }
            };

            switch (settingsType)
            {
                case SettingsType.Sandbox:
                    {
                        settings.Add("url", "https://www.testlitle.com/sandbox/communicator/online");
                        settings.Add("username", "MyUsername");
                        settings.Add("password", "MyPassword");
                        settings.Add("merchantId", "MyMerchantId");
                        settings.Add("reportGroup", "Money2020");
                        break;
                    }
                case SettingsType.Prelive:
                    {
                        settings.Add("url", "https://payments.vantivprelive.com/vap/communicator/online");
                        settings.Add("username", "MyUsername");
                        settings.Add("password", "MyPassword");
                        settings.Add("merchantId", "MyMerchantId");
                        settings.Add("reportGroup", "Money2020");
                        settings.Add("knownHostsFile", Settings.Default.knownHostsFile);
                        settings.Add("requestDirectory", Settings.Default.requestDirectory);
                        settings.Add("responseDirectory", Settings.Default.responseDirectory);
                        settings.Add("sftpUrl", Settings.Default.sftpUrl);
                        settings.Add("sftpUsername", "MyUsername");
                        settings.Add("sftpPassword", "MyPassword");
                        break;
                    }
            }

            return settings;

            //********************** Possible configurations **************************
            //this.config["url"] = Settings.Default.url;
            //this.config["reportGroup"] = Settings.Default.reportGroup;
            //this.config["username"] = Settings.Default.username;
            //this.config["printxml"] = Settings.Default.printxml;
            //this.config["timeout"] = Settings.Default.timeout;
            //this.config["proxyHost"] = Settings.Default.proxyHost;
            //this.config["merchantId"] = Settings.Default.merchantId;
            //this.config["password"] = Settings.Default.password;
            //this.config["proxyPort"] = Settings.Default.proxyPort;
            //this.config["logFile"] = Settings.Default.logFile;
            //this.config["neuterAccountNums"] = Settings.Default.neuterAccountNums;

            //config["url"] = Properties.Settings.Default.url;
            //config["reportGroup"] = Properties.Settings.Default.reportGroup;
            //config["username"] = Properties.Settings.Default.username;
            //config["printxml"] = Properties.Settings.Default.printxml;
            //config["timeout"] = Properties.Settings.Default.timeout;
            //config["proxyHost"] = Properties.Settings.Default.proxyHost;
            //config["merchantId"] = Properties.Settings.Default.merchantId;
            //config["password"] = Properties.Settings.Default.password;
            //config["proxyPort"] = Properties.Settings.Default.proxyPort;
            //config["sftpUrl"] = Properties.Settings.Default.sftpUrl;
            //config["sftpUsername"] = Properties.Settings.Default.sftpUsername;
            //config["sftpPassword"] = Properties.Settings.Default.sftpPassword;
            //config["knownHostsFile"] = Properties.Settings.Default.knownHostsFile;
            //config["requestDirectory"] = Properties.Settings.Default.requestDirectory;
            //config["responseDirectory"] = Properties.Settings.Default.responseDirectory;
        }

        private static void ShowLitleBatchResponse(litleResponse litleResponse)
        {
            Console.WriteLine("************************ litleResponse **************************");
            litleResponse.SerializeJson();

            batchResponse batchResponse = litleResponse.nextBatchResponse();
            while (batchResponse != null)
            {
                Console.WriteLine("************************ batchResponse **************************");
                batchResponse.SerializeJson();

                var accountUpdateResponseData = batchResponse.nextAccountUpdateResponse();
                while (accountUpdateResponseData != null)
                {
                    Console.WriteLine("************************ accountUpdateResponse **************************");
                    accountUpdateResponseData.SerializeJson();
                    accountUpdateResponseData = batchResponse.nextAccountUpdateResponse();
                }

                batchResponse = litleResponse.nextBatchResponse();
            }
        }

        private static void SerializeJson(this object value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            Console.WriteLine(json);
        }

        #endregion Utils
    }
}