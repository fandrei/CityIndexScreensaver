using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
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

			State.Data.SubscribePrices(ApplicationSettings.Instance.PricesToWatch[0], OnGraphUpdate);

			State.Data.SubscribeNews(news => NewsTicker.DataContext = news);
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

		private void OnGraphUpdate(PriceDTO val)
		{
			var time = DateTime.Now; // NOTE this code is temporary workaround until datetime bug is fixed
			var item = new GraphControl.Item {Value = val.Price, Time = time};
			ViewPanel.AddItem(item);
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

		// mouse movement detection
		private DateTime? _startTime;
		private Point? _prevMousePosition;
		private const double MouseMovementThreshold = 10;
		private const int IgnoreMouseSecs = 3;
	}
}
