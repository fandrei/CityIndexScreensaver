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

namespace CIAPI.IntegrationTests
{
	[TestFixture]
	public class MassStreamingTests
	{
		[Test]
		public void TestSubscribeMultiplePrices()
		{
			const string userName = "0x234";
			const string password = "password";

			var client = new Client(RPC_URI);
			client.LogIn(userName, password);

			var streamingClient = StreamingClientFactory.CreateStreamingClient(
				STREAMING_URI, userName, client.Session);

			streamingClient.Connect();

			var sync = new object();
			var listeners = new List<IStreamingListener<PriceDTO>>();
			try
			{
				var i = 0;
				var options = new ParallelOptions { MaxDegreeOfParallelism = 1 };
				Parallel.ForEach(Const.MarketIds, options,
					marketId =>
					{
						lock (sync)
						{
							var listener = streamingClient.BuildPricesListener(new[] { marketId });
							listeners.Add(listener);
							listener.MessageReceived += listener_MessageReceived;
							listener.Start();
						}
						Debug.WriteLine("item: {0}", i++);
					});
				Debug.WriteLine("\r\nAll listeners created ok\r\n");
			}
			finally
			{
				var i = 0;
				foreach (var listener in listeners)
				{
					listener.Stop();
					Debug.WriteLine("unsubscribed item: {0}", i++);
				}
				streamingClient.Disconnect();
				client.LogOut();
			}
		}

		void listener_MessageReceived(object sender, MessageEventArgs<PriceDTO> e)
		{
		}

		private static Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING");
		private static Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/TradingApi/");
		private ILog _logger = LogManager.GetCurrentClassLogger();
	}
}
