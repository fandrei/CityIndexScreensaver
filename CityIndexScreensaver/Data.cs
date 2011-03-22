using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

		public void SubscribeNews(Action<NewsDTO[]> onUpdate)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(x => SubscribeNewsThreadEntry(onUpdate));
		}

		public void SubscribePriceTicks(string topic, Action<PriceTickDTO> onUpdate)
		{
			VerifyIfDisposed();
			//ThreadPool.QueueUserWorkItem(x => SubscribePriceTicksThreadEntry(topic, onUpdate));
			ThreadPool.QueueUserWorkItem(x => GenerateDummyPriceTicksThreadEntry(onUpdate));
		}

		public void SubscribePrices(string topic, Action<PriceDTO> onUpdate)
		{
			VerifyIfDisposed();
			ThreadPool.QueueUserWorkItem(x => SubscribePricesThreadEntry(topic, onUpdate));
		}

		// call from background threads only
		void EnsureConnection()
		{
			lock (_syncIsDisposed)
			{
				if (_isDisposed)
					Thread.CurrentThread.Abort();
			}

			try
			{
				lock (_sync)
				{
					if (_client == null)
					{
						_client = new Client(new Uri(ApplicationSettings.Instance.ServerUrl));
						_client.LogIn(USERNAME, PASSWORD);
					}

					if (_streamingClient == null)
					{
						_streamingClient = StreamingClientFactory.CreateStreamingClient(
							new Uri(ApplicationSettings.Instance.StreamingServerUrl), USERNAME, _client.SessionId);
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
				var listener = _streamingClient.BuildPriceListener(topic);
				listener.MessageReceived +=
					(s, args) =>
					{
						try
						{
							var val = args.Data;
							//Debug.WriteLine("\r\n--------------------------------------\r\n");
							//Debug.WriteLine("Price: {0} {1} {2}\r\n", val.MarketId, val.Price, val.TickDate);
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
			EnsureConnection();

			try
			{
				var resp = _client.ListNewsHeadlines("UK", 20);
				onSuccess(resp.Headlines);

			}
			catch (Exception exc)
			{
				_onError(exc);
			}
		}

		readonly object _syncIsDisposed = new object();
		private bool _isDisposed;

		public void Dispose()
		{
			Debug.WriteLine("Data.Dispose()\r\n");
			lock (_syncIsDisposed)
			{
				_isDisposed = true;
			}

			try
			{
				var listeners = new List<IStreamingListener>();

				lock (_sync)
				{
					listeners.AddRange(_priceListeners);
					_priceListeners.Clear();
				}

				lock (_sync)
				{
					listeners.AddRange(_priceTicksListeners);
					_priceTicksListeners.Clear();
				}

				Parallel.ForEach(listeners, listener => listener.Stop());

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

		void VerifyIfDisposed()
		{
			lock (_syncIsDisposed)
			{
				if (_isDisposed)
					throw new ObjectDisposedException("Data");
			}
		}

		private const string USERNAME = "xx189949";
		private const string PASSWORD = "password";

		readonly object _sync = new object();
		private Client _client;
		private IStreamingClient _streamingClient;

		private readonly List<IStreamingListener<PriceDTO>> _priceListeners = new List<IStreamingListener<PriceDTO>>();
		private readonly List<IStreamingListener<PriceTickDTO>> _priceTicksListeners =
			new List<IStreamingListener<PriceTickDTO>>();
	}
}
