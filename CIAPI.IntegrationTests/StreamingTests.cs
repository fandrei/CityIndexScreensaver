using System;
using System.Collections.Generic;
using System.Threading;
using CIAPI.DTO;
using CIAPI.Rpc;
using CIAPI.Streaming;
using Common.Logging;
using NUnit.Framework;
using StreamingClient;

namespace CIAPI.IntegrationTests
{
    /// <summary>
    /// This relates to #25 - https://github.com/cityindex/ciapi.cs/issues#issue/25
    /// </summary>
    [TestFixture]
    public class StreamingTests
    {
        private static Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING");
        private static Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/TradingApi/");
        private ILog _logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void DoesNotListenForMessagesOfWrongTypeOnTopic()
        {
            const int MARKET = 71442;
            var maxWaitTime = TimeSpan.FromSeconds(10);

            var streamingClient = BuildStreamingClient();
            streamingClient.Connect();

            var priceListener = streamingClient.BuildListener<PriceDTO>("PRICES.PRICE." + MARKET);
            var priceBarListener = streamingClient.BuildListener<PriceBarDTO>("PRICES.PRICE." + MARKET); //PriceBars are not sent over this topic
            var priceTickListener = streamingClient.BuildListener<PriceTickDTO>("PRICES.PRICE." + MARKET); //PriceTicks are not sent over this topic
            var prices = new List<PriceDTO>();
            var priceBars = new List<PriceBarDTO>();
            var priceTicks = new List<PriceTickDTO>();
            try
            {
                BeginCollectingMessages(priceListener, prices);
                BeginCollectingMessages(priceBarListener,  priceBars);
                BeginCollectingMessages(priceTickListener, priceTicks);

                Thread.Sleep(maxWaitTime);

                Assert.That(prices.Count, Is.GreaterThanOrEqualTo(1), "Not enough prices");
                Assert.That(priceBars.Count, Is.EqualTo(0), "Too many pricebars");
                Assert.That(priceTicks.Count, Is.EqualTo(0), "Too many priceTicks");
            }
            finally
            {
                priceListener.Stop();
                priceBarListener.Stop();
                priceTickListener.Stop();
                streamingClient.Disconnect();

                LogMessagesRecieved("Prices", prices);
                LogMessagesRecieved("Price Bars", priceBars);
                LogMessagesRecieved("Price Ticks", priceTicks);
            }
        }


        #region Helper methods

        private static IStreamingClient BuildStreamingClient(
            string userName = "0x234",
            string password = "password")
        {
            var authenticatedClient = new Client(RPC_URI);
            authenticatedClient.LogIn(userName, password);

            return StreamingClientFactory.CreateStreamingClient(STREAMING_URI, userName, authenticatedClient.SessionId);
        }

        private void LogMessagesRecieved<T>(string title, IEnumerable<T> messages)
        {
            _logger.InfoFormat("=============> {0} :", title);
            foreach (var message in messages)
            {
                _logger.Info("\t" + message.ToStringWithValues());
            }
        }

        private void BeginCollectingMessages<T>(IStreamingListener<T> listener, ICollection<T> messages) where T : class
        {
            listener.MessageRecieved += (s, e) =>
                                            {
                                                messages.Add(e.Data);
                                                _logger.Info("++ Recieved message " + e.Data.ToStringWithValues());
                                            };
            listener.Start();
        }

        #endregion

        [Test]
        public void DoesNotGetDuplicatedPrices()
        {
            /* 24/5 fast pricing FX markets

            {"MarketId":400481115,"Name":"AUD\/JPY"},
            {"MarketId":400481116,"Name":"AUD\/NZD"},
            {"MarketId":400481117,"Name":"AUD\/USD"},
            {"MarketId":400481118,"Name":"CAD\/CHF"},
            {"MarketId":400481119,"Name":"CAD\/JPY"},
            {"MarketId":400481120,"Name":"CHF\/JPY"},
            {"MarketId":400481121,"Name":"EUR\/AUD"},
            {"MarketId":400481122,"Name":"EUR\/CAD"},
             */
            var markets = new[] { 400481115, 400481116, 400481117, 400481118, 400481119, 400481120, 400481121, 400481122 };

            var streamingClient = BuildStreamingClient();
            streamingClient.Connect();
            
            var capturedPrices = ListenToPricesFor(streamingClient, markets, TimeSpan.FromSeconds(30), 10);
            streamingClient.Disconnect();

            AssertNoDuplicatePrices(capturedPrices);

        }

        #region Helper methods

        private static void AssertNoDuplicatePrices(Dictionary<int, List<PriceDTO>> capturedPrices)
        {
            foreach (var market in capturedPrices.Keys)
            {
                foreach (var price in capturedPrices[market])
                {
                    Assert.That(price.MarketId, Is.EqualTo(market), string.Format("The list of prices for market {0} contains a price for a different market {1}", market, price.MarketId));
                    foreach (var capturedPrice in capturedPrices)
                    {
                        if (capturedPrice.Key != market)
                        {
                            foreach (var otherMarketsPrice in capturedPrices[capturedPrice.Key])
                            {
                                Assert.That(price.AuditId, Is.Not.EqualTo(otherMarketsPrice.AuditId), string.Format("Duplicate price found.  Price for market {0} has same auditId as price for market {1}", market, capturedPrice.Key));
                            }
                        }
                    }
                }
            }
        }

        private static Dictionary<int, List<PriceDTO>> ListenToPricesFor(IStreamingClient streamingClient,
                                                                         IEnumerable<int> markets, TimeSpan within, int minPricesToCollect)
        {
            var gates = new Dictionary<int, ManualResetEvent>();
            var priceListeners = new Dictionary<int, IStreamingListener>();
            var collectedPrices = new Dictionary<int, List<PriceDTO>>();
            foreach (var market in markets)
            {
                var gate = new ManualResetEvent(false);
                gates.Add(market, gate);
                var prices = new List<PriceDTO>();
                collectedPrices.Add(market, prices);

                var priceListener = streamingClient.BuildListener<PriceDTO>("PRICES.PRICE." + market);
                priceListener.MessageRecieved += (s, e) =>
                                                     {
                                                         prices.Add(e.Data);
                                                         if (prices.Count >= minPricesToCollect) gate.Set();
                                                     };

                priceListeners.Add(market, priceListener);
                priceListener.Start();
            }


            try
            {
                foreach (var market in markets)
                {
                    if (!gates[market].WaitOne(within))
                    {
                        Assert.Fail("Not enough prices were collected for {0} within {1}.  Required {2}.  Got {3}",
                                    market, within, minPricesToCollect, collectedPrices[market].Count);
                    }
                }
            }
            finally
            {
                foreach (var market in markets)
                {
                    priceListeners[market].Stop();
                }
            }

            return collectedPrices;
        }

        #endregion

    }
}
