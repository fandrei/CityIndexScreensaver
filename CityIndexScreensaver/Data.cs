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
			if (exc is ThreadAbortException)
				return;
#if DEBUG
			//Debugger.Break();
#endif
			Callback(_onError, exc);
		}

		void Callback(Action callback)
		{
			Application.Current.Dispatcher.Invoke(callback);
		}

		void Callback<T>(Action<T> callback, T val)
		{
			Action action = () => callback(val);
			Application.Current.Dispatcher.Invoke(action);
		}

		public void SubscribeNews(Action<NewsDTO> onUpdate)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(x => SubscribeNewsThreadEntry(onUpdate));
		}

		void SubscribeNewsThreadEntry(Action<NewsDTO> onUpdate)
		{
			try
			{
				ListNewsHeadlinesResponseDTO resp;
				lock (_sync)
				{
					EnsureConnectionSync();

					var listener = _streamingClient.BuildNewsHeadlinesListener(ApplicationSettings.Instance.NewsCategory);
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

					resp = _client.ListNewsHeadlines(ApplicationSettings.Instance.NewsCategory,
						ApplicationSettings.Instance.NewsMaxCount);
					foreach (var val in resp.Headlines)
					{
						Callback(onUpdate, val);
					}
				}
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
		}

		public void SubscribePrices(int id, Action<PriceDTO> onUpdate)
		{
			VerifyIfDisposed();
			var topic = "PRICES.PRICE." + id;

			ThreadPool.QueueUserWorkItem(x => SubscribePricesThreadEntry(topic, onUpdate));
			//ThreadPool.QueueUserWorkItem(x => GenerateDummyPricesThreadEntry(id, onUpdate));
		}

		void SubscribePricesThreadEntry(string topic, Action<PriceDTO> onUpdate)
		{
			try
			{
				lock (_sync)
				{
					EnsureConnectionSync();

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

		void GenerateDummyPricesThreadEntry(int id, Action<PriceDTO> onUpdate)
		{
			try
			{
				var random = new Random(id);
				const int maxRefreshDelay = 5000;
				const int maxPrice = 10000;

				var price = Convert.ToDecimal(random.NextDouble() * maxPrice);
				var startPrice = (double)price;
				price = Math.Round(price, 2);
				decimal delta = 0;
				decimal min = price, max = price;

				while (!_disposing)
				{
					var data = new PriceDTO
					{
						Price = price,
						Low = min,
						High = max,
						Bid = price,
						Offer = price,
						MarketId = id,
						Change = Math.Round(delta, 2)
					};

					Callback(onUpdate, data);

					delta = Convert.ToDecimal((random.NextDouble() - 0.5) * startPrice * 0.01);
					price += delta;
					if (price <= 0)
						price = Math.Abs(price + delta);
					price = Math.Round(price, 2);
					min = Math.Min(min, price);
					max = Math.Max(max, price);

					Thread.Sleep(random.Next(maxRefreshDelay));
				}
			}
			catch (Exception exc)
			{
				ReportError(exc);
			}
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

				//EnsureConnectionSync();
				//var accountInfo = _client.GetClientAndTradingAccount();
				//var resp = _client.ListSpreadMarkets("", "", accountInfo.ClientAccountId, -1);
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

		public PriceBarDTO[] GetPriceBar(int marketId)
		{
			EnsureConnectionSync();
			var resp = _client.GetPriceBars(marketId.ToString(), "DAY", 1, 1.ToString());
			return resp.PriceBars;
		}

		public void EnsureConnection(Action onSuccess, Action onError)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(
				x =>
				{
					try
					{
						EnsureConnectionSync();
						Callback(onSuccess);
					}
					catch (Exception exc)
					{
						Callback(onError);
						ReportError(exc);
					}
				});
		}

		void EnsureConnectionSync()
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

		public void Logout(Action onSuccess)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(
				x =>
				{
					try
					{
						LogoutSync();
						Callback(onSuccess);
					}
					catch (Exception exc)
					{
						ReportError(exc);
					}
				});
		}

		void LogoutSync()
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

		private volatile bool _disposing;

		public void Dispose()
		{
			Debug.WriteLine("Data.Dispose()\r\n");
			_disposing = true;

			try
			{
				LogoutSync();
				Debug.WriteLine("Data.Dispose() finished successfully\r\n");
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Data.Dispose() error:\r\n{0}", exc.ToString());
			}
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
