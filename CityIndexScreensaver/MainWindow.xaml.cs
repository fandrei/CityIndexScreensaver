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
			WindowStyle = WindowStyle.None;
			WindowState = WindowState.Maximized;
		}

		private void ReportException(Exception exc)
		{
			var msg = State.IsDebug ? exc.ToString() : exc.Message;
			if (!string.IsNullOrEmpty(ErrorTextBlock.Text))
				ErrorTextBlock.Text += "\r\n-----------------------------------------------\r\n";
			ErrorTextBlock.Text += msg;
			ErrorPopup.Visibility = Visibility.Visible;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			CloseApp();
		}

		private void Window_MouseMove(object sender, MouseEventArgs e)
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

			foreach (var market in prices)
			{
				var id = market.MarketId;
				var priceInfo = new PriceInfo { MarketId = id, MarketName = market.Name };
				priceInfo.Color = new SolidColorBrush(PriceGraph.GetGraphColor(id.ToString()));
				priceInfoList.Add(priceInfo);

				PriceGraphSingle.SetGraphBrush(priceInfo.MarketId.ToString(), priceInfo.Color);

				State.Data.SubscribePrices(id,
					price =>
					{
						//var priceBar = State.Data.GetPriceBar(price.MarketId);

						priceInfo.Price = price.Price;
						priceInfo.Change = price.Change;
						OnGraphUpdate(price);
					});
			}

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
			// NOTE this code is a temporary workaround until datetime bug is fixed
			var ticksString = val.TickDate.Substring(7, val.TickDate.Length - 10);
			var time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(Int64.Parse(ticksString));

			var item = new GraphItem { Value = (double)val.Price, Time = time };
			var key = val.MarketId.ToString();

			PriceGraph.AddItem(key, item);
			PriceGraphSingle.AddItem(key, item);
		}

		private void PricesView_SelectedChanged(object sender, SelectedPriceChangedArgs e)
		{
			PriceGraphSingle.SetCurrentVisible(e.Val.MarketId.ToString());
		}

		// mouse movement detection
		private DateTime? _startTime;
		private Point? _prevMousePosition;
		private const double MouseMovementThreshold = 10;
		private const int IgnoreMouseSecs = 3;
	}
}
