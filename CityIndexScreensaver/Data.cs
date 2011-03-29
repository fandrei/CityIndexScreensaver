using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using CIAPI.DTO;
using CIAPI.Rpc;
using CIAPI.Streaming;
using StreamingClient;
using IStreamingClient = CIAPI.Streaming.IStreamingClient;

namespace CityIndexScreensaver
{
	class Data : IDisposable
	{
		public Data(Action<Exception> onError)
		{
			_onError = onError;
		}

		private readonly Action<Exception> _onError;

		void ReportError(Exception exc)
		{
#if DEBUG
			Debugger.Break();
#endif
			Callback(_onError, exc);
		}

		void Callback<T>(Action<T> callback, T val)
		{
			Action action = () => callback(val);
			Application.Current.Dispatcher.BeginInvoke(action);
		}

		public void SubscribeNews(Action<NewsDTO> onUpdate)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(x => SubscribeNewsThreadEntry("NEWS.MOCKHEADLINES.UK", onUpdate));
		}

		public void SubscribePrices(int id, Action<PriceDTO> onUpdate)
		{
			VerifyIfDisposed();
			var topic = "PRICES.PRICE." + id;

			ThreadPool.QueueUserWorkItem(x => SubscribePricesThreadEntry(topic, onUpdate));
			//ThreadPool.QueueUserWorkItem(x => GenerateDummyPricesThreadEntry(topic, onUpdate));
		}

		public void GetMarketsList(Action<MarketDTO[]> onSuccess)
		{
			ThreadPool.QueueUserWorkItem(x => GetMarketsListThreadEntry(onSuccess));
		}

		void GetMarketsListThreadEntry(Action<MarketDTO[]> onSuccess)
		{
			try
			{
				// non-functional yet...

				//lock (_sync)
				//{
				//    EnsureConnection();
				//}
				//var accountInfo = _client.GetClientAndTradingAccount();
				//var resp = _client.ListCfdMarkets("", "", accountInfo.ClientAccountId, -1);
				//Callback(onSuccess, resp.Markets);

				var markets = new List<MarketDTO>(Const.Markets.Count);
				markets.AddRange(Const.Markets.Select(pair => new MarketDTO { MarketId = pair.Key, Name = pair.Value }));
				Callback(onSuccess, markets.ToArray());
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
		}

		void EnsureConnection()
		{
			if (_disposing)
				Thread.CurrentThread.Abort();

			lock (_sync)
			{
				if (_client == null)
				{
					_client = new Client(new Uri(ApplicationSettings.Instance.ServerUrl));
					_client.LogIn(ApplicationSettings.Instance.UserName, ApplicationSettings.Instance.Password);
				}

				if (_streamingClient == null)
				{
					_streamingClient = StreamingClientFactory.CreateStreamingClient(
						new Uri(ApplicationSettings.Instance.StreamingServerUrl), 
						ApplicationSettings.Instance.UserName, _client.SessionId);
					_streamingClient.Connect();
				}
			}
		}

		void SubscribePricesThreadEntry(string topic, Action<PriceDTO> onUpdate)
		{
			try
			{
				lock (_sync)
				{
					EnsureConnection();

					var listener = _streamingClient.BuildPriceListener(topic);
					listener.MessageReceived +=
						(s, args) =>
						{
							try
							{
								Callback(onUpdate, args.Data);
							}
							catch (Exception exc)
							{
								ReportError(exc);
							}
						};
					listener.Start();

					_priceListeners.Add(listener);
				}
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
		}

		void GenerateDummyPricesThreadEntry(string topic, Action<PriceDTO> onUpdate)
		{
			try
			{
				var random = new Random();
				var price = Convert.ToDecimal(random.NextDouble() * 5000);
				price = Math.Round(price, 2);
				decimal min = price, max = price;
				while (!_disposing)
				{
					var data = new PriceDTO { Price = price, Low = min, High = max, Bid = price, Offer = price };

					Callback(onUpdate, data);

					var delta = Convert.ToDecimal(random.NextDouble() * 300 - 150);
					price += delta;
					if (price <= 0)
						price = Math.Abs(price + delta);
					price = Math.Round(price, 2);
					min = Math.Min(min, price);
					max = Math.Max(max, price);

					Thread.Sleep(random.Next(5000));
				}
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
		}

		void SubscribeNewsThreadEntry(string topic, Action<NewsDTO> onUpdate)
		{
			try
			{
				ListNewsHeadlinesResponseDTO resp;
				lock (_sync)
				{
					EnsureConnection();

					resp = _client.ListNewsHeadlines(topic, 20);

					var listener = _streamingClient.BuildNewsHeadlinesListener(topic);
					listener.MessageReceived +=
						(s, args) =>
						{
							try
							{
								var val = args.Data;
								Callback(onUpdate, val);
							}
							catch (Exception exc)
							{
								ReportError(exc);
							}
						};
					listener.Start();
				}
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
		}

		private volatile bool _disposing;

		public void Dispose()
		{
			Debug.WriteLine("Data.Dispose()\r\n");
			_disposing = true;

			try
			{
				lock (_sync)
				{
					var listeners = new List<IStreamingListener>();
					listeners.AddRange(_priceListeners);
					_priceListeners.Clear();

					foreach (var listener in listeners)
					{
						listener.Stop();
					}

					if (_streamingClient != null)
					{
						_streamingClient.Disconnect();
						_streamingClient = null;
					}
					if (_client != null)
					{
						_client.LogOut();
						_client.Dispose();
						_client = null;
					}
				}
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}

			Debug.WriteLine("Data.Dispose() finished successfully\r\n");
		}

		void VerifyIfDisposed()
		{
			if (_disposing)
				throw new ObjectDisposedException("Data");
		}

		readonly object _sync = new object();
		private Client _client;

		private IStreamingClient _streamingClient;

		private readonly List<IStreamingListener<PriceDTO>> _priceListeners = new List<IStreamingListener<PriceDTO>>();
	}
}
