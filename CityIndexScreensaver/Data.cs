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
		public void GetData(Action onSuccess, Action<Exception> onError)
		{
			ThreadPool.QueueUserWorkItem(x => GetDataThreadEntry(onSuccess, onError));
		}

		void GetDataThreadEntry(Action onSuccess, Action<Exception> onError)
		{
			try
			{
				_client = new Client(RPC_URI);

				_client.LogIn(USERNAME, PASSWORD);

				_streamingClient = StreamingClientFactory.CreateStreamingClient(
					STREAMING_URI, USERNAME, _client.SessionId);
				_streamingClient.Connect();

				_streamListener = _streamingClient.BuildListener<PriceDTO>("PRICES.PRICE.400481147");
				_streamListener.Start();
				_streamListener.MessageRecieved +=
					(s, e) =>
					{
						Debug.WriteLine(e.ToString());
					};
			}
			catch (Exception exc)
			{
				onError(exc);
			}
		}

		public void Dispose()
		{
			if (_streamListener != null)
			{
				_streamListener.Stop();
				_streamListener = null;
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
		private IStreamingListener<PriceDTO> _streamListener;
	}
}
