using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CIAPI.DTO;
using Common.Logging;
using NUnit.Framework;

using CIAPI.Rpc;
using CIAPI.Streaming;
using StreamingClient;
using IStreamingClient = CIAPI.Streaming.IStreamingClient;

namespace CIAPI.IntegrationTests
{
	[TestFixture]
	public class MassStreamingTests
	{
		[Test]
		public void TestSubscribeMultiplePrices()
		{
			var streamingClient = BuildStreamingClient();
			streamingClient.Connect();

			var sync = new object();
			var listeners = new List<IStreamingListener<PriceDTO>>();
			try
			{
				lock (sync)
				{
					foreach (var marketId in Const.MarketIds)
					{
						var topic = string.Format("PRICES.PRICE.{0}", marketId);
						var listener = streamingClient.BuildPriceListener(topic);
						listeners.Add(listener);
					}
				}

				lock (sync)
				{
					var i = 0;
					var options = new ParallelOptions { MaxDegreeOfParallelism = -1 };
					Parallel.ForEach(listeners, options,
						listener =>
						{
							listener.MessageReceived += listener_MessageReceived;
							listener.Start();
							Debug.WriteLine("item: {0}", i++);
						});
					Debug.WriteLine("\r\nAll listeners created ok\r\n");
				}
			}
			finally
			{
				foreach (var listener in listeners)
				{
					listener.Stop();
				}
				streamingClient.Disconnect();
			}
		}

		void listener_MessageReceived(object sender, MessageEventArgs<PriceDTO> e)
		{
		}

		private static IStreamingClient BuildStreamingClient(
	string userName = "0x234",
	string password = "password")
		{
			var authenticatedClient = new Client(RPC_URI);
			authenticatedClient.LogIn(userName, password);

			return StreamingClientFactory.CreateStreamingClient(STREAMING_URI, userName, authenticatedClient.SessionId);
		}

		private static Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING");
		private static Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/TradingApi/");
		private ILog _logger = LogManager.GetCurrentClassLogger();
	}
}
