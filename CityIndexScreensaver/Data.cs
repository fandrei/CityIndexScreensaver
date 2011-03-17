using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CIAPI.DTO;
using CIAPI.Rpc;
using CIAPI.Streaming;
using StreamingClient;

namespace CityIndexScreensaver
{
	class Data : IDisposable
	{
		public Data(Action<Exception> onError)
		{
			_onError = onError;
		}

		private readonly Action<Exception> _onError;

		public void SubscribeNews(Action<NewsDTO[]> onUpdate)
		{
			ThreadPool.QueueUserWorkItem(x => SubscribeNewsThreadEntry(onUpdate));
		}

		public void SubscribePriceTicks(Action<PriceTickDTO> onUpdate)
		{
			//const string topic = "PRICES.PRICE.154297";
			//const string topic = "PRICES.PRICE.71442";
			const string topic = "PRICES.PRICE.99498";
			//ThreadPool.QueueUserWorkItem(x => SubscribePriceTicksThreadEntry(topic, onUpdate));
			ThreadPool.QueueUserWorkItem(x => GenerateDummyPriceTicksThreadEntry(onUpdate));
		}

		public void SubscribePrices(string topic, Action<PriceDTO> onUpdate)
		{
			ThreadPool.QueueUserWorkItem(x => SubscribePricesThreadEntry(topic, onUpdate));
		}

		void EnsureConnection()
		{
			try
			{
				lock (_sync)
				{
					if (_client == null)
					{
						_client = new Client(RPC_URI);
						_client.LogIn(USERNAME, PASSWORD);
					}

					if (_streamingClient == null)
					{
						_streamingClient = StreamingClientFactory.CreateStreamingClient(
							STREAMING_URI, USERNAME, _client.SessionId);
						_streamingClient.Connect();
					}
				}
			}
			catch (Exception exc)
			{
				_onError(exc);
			}
		}

		void SubscribePricesThreadEntry(string topic, Action<PriceDTO> onUpdate)
		{
			EnsureConnection();

			try
			{
				var listener = _streamingClient.BuildListener<PriceDTO>(topic);
				listener.MessageRecieved +=
					(s, args) =>
					{
						try
						{
							var val = args.Data;
							Debug.WriteLine("\r\n--------------------------------------\r\n");
							Debug.WriteLine("Price: {0} {1} {2}\r\n", val.MarketId, val.Price, val.TickDate);
							onUpdate(val);
						}
						catch (Exception exc)
						{
							_onError(exc);
						}
					};
				listener.Start();

				lock (_sync)
				{
					_priceListeners.Add(listener);
				}
			}
			catch (Exception exc)
			{
				_onError(exc);
			}
		}

		void GenerateDummyPriceTicksThreadEntry(Action<PriceTickDTO> onUpdate)
		{
			try
			{
				var random = new Random();
				var price = Convert.ToDecimal(random.NextDouble() * 10000);
				while (true)
				{
					var data = new PriceTickDTO { Price = price, TickDate = DateTime.Now };
					var delta = Convert.ToDecimal(random.NextDouble() * 1000 - 500);
					price += delta;
					if (price <= 0)
						price = Math.Abs(price + delta);
					onUpdate(data);
					Thread.Sleep(random.Next(1000));
				}
			}
			catch (Exception exc)
			{
				_onError(exc);
			}
		}

		void SubscribeNewsThreadEntry(Action<NewsDTO[]> onSuccess)
		{
			try
			{
				var client = new Client(RPC_URI);
				client.LogIn(USERNAME, PASSWORD);
				var resp = client.ListNewsHeadlines("UK", 20);
				client.LogOut();

				onSuccess(resp.Headlines);

			}
			catch (Exception exc)
			{
				_onError(exc);
			}
		}

		public void Dispose()
		{
			Debug.WriteLine("Data.Dispose()\r\n");

			try
			{
				lock (_sync)
				{
					foreach (var listener in _priceListeners)
					{
						if (listener != null)
						{
							listener.Stop();
						}
					}
					_priceListeners.Clear();
				}

				lock (_sync)
				{
					if (_streamingClient != null)
					{
						_streamingClient.Disconnect();
						_streamingClient = null;
					}
					if (_client != null)
					{
						_client.BeginLogOut(ar => _client.EndLogOut(ar), null);
						_client = null;
					}
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}
		}

		private static readonly Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/tradingapi");
		private static readonly Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING");

		private const string USERNAME = "xx189949";
		private const string PASSWORD = "password";

		object _sync = new object();
		private Client _client;
		private IStreamingClient _streamingClient;

		private List<IStreamingListener<PriceDTO>> _priceListeners = new List<IStreamingListener<PriceDTO>>();
	}
}
