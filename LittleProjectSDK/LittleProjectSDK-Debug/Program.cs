using Litle.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LittleProjectSDK_Debug
{
    internal static class Program
    {
        private static LitleOnline litle = new LitleOnline(GetSettings());

        private static void Main(string[] args)
        {
            contact contact = new contact
            {
                name = "John Smith",
                addressLine1 = "1 Main St.",
                city = "Burlington",
                state = "MA",
                zip = "01803",
                country = countryTypeEnum.US          
            };

            cardType card = new cardType
            {
                type = methodOfPaymentTypeEnum.VI,
                number = "4100423312345000",
                expDate = "0112",
                cardValidationNum = "349"
            };

            DoAuthorization(contact, card);
            DoSale(contact, card);

            Console.ReadLine();
        }

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

        private static filteringType GetFilteringType()
        {
            var filteringType = new filteringType
            {
                prepaid = true
            };

            return filteringType;
        }

        private static Dictionary<string, string> GetSettings()
        {
            var settings = new Dictionary<string, string>
            {
                { "url", "https://www.testlitle.com/sandbox/communicator/online" },
                { "username", "MyUsername" },
                { "password", "MyPassword" },
                { "merchantId", "MyMerchantId" },
                { "reportGroup", "Money2020" }
            };

            return settings;
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
        }
        private static void SerializeJson(this object value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}