using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using CIAPI.DTO;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			State.Data = new Data(ReportException);
			_startTime = DateTime.Now;

			Application.Current.DispatcherUnhandledException += OnUnhandledException;
			Application.Current.Exit += App_Unloaded;

			if (State.IsFullScreen)
				SetWindowFullScreen();

			SubscribePrices();
			SubscribeNews();
		}

		private void App_Unloaded(object sender, ExitEventArgs e)
		{
			State.Data.Dispose();
		}

		void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			try
			{
				ReportException(args.Exception);
			}
			catch (Exception exc)
			{
				Debugger.Break();
			}

			args.Handled = true;
		}

		private void SetWindowFullScreen()
		{
			Topmost = true;
			ResizeMode = ResizeMode.NoResize;

			Left = 0;
			Top = 0;
			Width = SystemParameters.PrimaryScreenWidth;
			Height = SystemParameters.PrimaryScreenHeight;
		}

		private void ReportException(Exception exc)
		{
			var msg = State.IsDebug ? exc.ToString() : exc.Message;
			ErrorTextBlock.Text = msg;
			ErrorPopup.Visibility = Visibility.Visible;
		}

		private void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			CloseApp();
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (!State.IsDebug)
			{
				var position = e.GetPosition(this);
				if (_startTime != null && (DateTime.Now - _startTime.Value).TotalSeconds > IgnoreMouseSecs)
				{
					// avoid false-positive triggering
					if (_prevMousePosition != null &&
						(Math.Abs(_prevMousePosition.Value.X - position.X) > MouseMovementThreshold ||
						Math.Abs(_prevMousePosition.Value.Y - position.Y) > MouseMovementThreshold))
					{
						CloseApp();
					}
				}
				_prevMousePosition = position;
			}
		}

		void CloseApp()
		{
			Application.Current.Shutdown();
		}

		private void SubscribePrices()
		{
			var prices = ApplicationSettings.Instance.PricesToWatch;
			var priceInfoList = new ObservableCollection<PriceInfo>();

			foreach (var id in prices)
			{
				var priceInfo = new PriceInfo { Id = id, MarketName = id.ToString() };
				priceInfo.Color = new SolidColorBrush(PriceGraph.GetGraphColor(id.ToString()));
				priceInfoList.Add(priceInfo);

				State.Data.SubscribePrices(id,
					price =>
					{
						priceInfo.Price = price.Price;
						priceInfo.Change = price.Change;
						OnGraphUpdate(price);

						// NOTE this is a workaround to enforce DataGrid adjust column widths
						PricesView.DataContext = null;
						PricesView.DataContext = priceInfoList;
					});
			}

			State.Data.GetMarketsList(
				markets =>
				{
					var marketNames = markets.ToDictionary(market => market.MarketId, market => market.Name);

					foreach (var priceInfo in priceInfoList)
					{
						string marketName;
						if (marketNames.TryGetValue(priceInfo.Id, out marketName))
						{
							priceInfo.MarketName = marketName;
						}
					}
				});

			PricesView.DataContext = priceInfoList;
		}

		private void SubscribeNews()
		{
			var news = new ObservableCollection<NewsDTO>();
			NewsTicker.DataContext = news;
			State.Data.SubscribeNews(
				val =>
				{
					news.Add(val);
					while (news.Count > ApplicationSettings.Instance.NewsMaxCount)
					{
						news.RemoveAt(0);
					}
				});
		}

		private void OnGraphUpdate(PriceDTO val)
		{
			var time = DateTime.Now; // NOTE this code is a temporary workaround until datetime bug is fixed
			var item = new GraphItem { Value = (double)val.Price, Time = time };
			PriceGraph.AddItem(val.MarketId.ToString(), item);
		}

		// mouse movement detection
		private DateTime? _startTime;
		private Point? _prevMousePosition;
		private const double MouseMovementThreshold = 10;
		private const int IgnoreMouseSecs = 3;
	}
}
