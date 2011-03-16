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
		public void GetNews(Action<NewsDTO[]> onUpdate, Action<Exception> onError)
		{
			ThreadPool.QueueUserWorkItem(x => GetNewsSyncThreadEntry(onUpdate, onError));
		}

		public void SubscribePrices(Action<PriceTickDTO> onUpdate, Action<Exception> onError)
		{
			const string topic = "PRICES.PRICE.154297";
			//const string topic = "PRICES.PRICE.71442";
			ThreadPool.QueueUserWorkItem(x => SubscribePricesThreadEntry(topic, onUpdate, onError));
			//ThreadPool.QueueUserWorkItem(x => GetDummyDataThreadEntry(onUpdate, onError));
		}

		void SubscribePricesThreadEntry(string topic, Action<PriceTickDTO> onUpdate, Action<Exception> onError)
		{
			try
			{
				_client = new Client(RPC_URI);

				_client.LogIn(USERNAME, PASSWORD);

				//var bars = _client.GetPriceBars("71442", "MINUTE", 1, "15");

				_streamingClient = StreamingClientFactory.CreateStreamingClient(
					STREAMING_URI, USERNAME, _client.SessionId);
				_streamingClient.Connect();

				//_priceListener = _streamingClient.BuildListener<PriceDTO>(topic);
				//_priceListener.MessageRecieved += priceListener_MessageRecieved;
				//_priceListener.Start();

				//_priceBarListener = _streamingClient.BuildListener<PriceBarDTO>(topic);
				//_priceBarListener.MessageRecieved += priceBarListener_MessageRecieved;
				//_priceBarListener.Start();

				_priceTicksListener = _streamingClient.BuildListener<PriceTickDTO>(topic);
				_priceTicksListener.MessageRecieved += priceTicksListener_MessageRecieved;
				_priceTicksListener.Start();
			}
			catch (Exception exc)
			{
				onError(exc);
			}
		}

		static void GetDummyDataThreadEntry(Action<PriceTickDTO> onUpdate, Action<Exception> onError)
		{
			try
			{
				var random = new Random();
				var price = Convert.ToDecimal(random.NextDouble()*10000);
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
				onError(exc);
			}
		}

		private void priceListener_MessageRecieved(object s, MessageEventArgs<PriceDTO> val)
		{
			Debug.WriteLine("\r\n--------------------------------------\r\n");
			Debug.WriteLine("Price: {0} {1}\r\n", val.Data.Price, val.Data.TickDate);
		}

		void priceBarListener_MessageRecieved(object sender, MessageEventArgs<PriceBarDTO> val)
		{
			Debug.WriteLine("\r\n--------------------------------------\r\n");
			Debug.WriteLine("PriceBar: {0} {1}\r\n", val.Data.Close, val.Data.BarDate);
		}

		void priceTicksListener_MessageRecieved(object sender, MessageEventArgs<PriceTickDTO> val)
		{
			Debug.WriteLine("\r\n--------------------------------------\r\n");
			Debug.WriteLine("PriceTick: {0} {1}\r\n", val.Data.Price, val.Data.TickDate);
		}

		public NewsDTO[] GetNewsDummy()
		{
			var dummyList = new List<NewsDTO>();
			for (int i = 0; i < 10; i++)
			{
				var text = "news " + i;
				for (int t = 0; t < 5; t++)
					text += text;
				dummyList.Add(
					new NewsDTO
					{
						Headline = text,
						PublishDate = DateTime.Now,
						StoryId = i
					});
			}

			return dummyList.ToArray();
		}

		void GetNewsSyncThreadEntry(Action<NewsDTO[]> onSuccess, Action<Exception> onError)
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
				onError(exc);
			}
		}

		public void Dispose()
		{
			if (_priceTicksListener != null)
			{
				_priceTicksListener.Stop();
				_priceTicksListener = null;
			}
			if (_priceBarListener != null)
			{
				_priceBarListener.Stop();
				_priceBarListener = null;
			}
			if (_priceListener != null)
			{
				_priceListener.Stop();
				_priceListener = null;
			}
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

		private static readonly Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/tradingapi");
		private static readonly Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING");

		private const string USERNAME = "xx189949";
		private const string PASSWORD = "password";

		private Client _client;
		private IStreamingClient _streamingClient;
		private IStreamingListener<PriceDTO> _priceListener;
		private IStreamingListener<PriceBarDTO> _priceBarListener;
		private IStreamingListener<PriceTickDTO> _priceTicksListener;
	}
}
