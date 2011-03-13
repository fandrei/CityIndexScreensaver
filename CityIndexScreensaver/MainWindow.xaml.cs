using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

		private void Grid_KeyDown(object sender, KeyEventArgs e)
		{
			CloseApp();
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (!State.IsDebug)
			{
				if (_startTime != DateTime.MinValue && (DateTime.Now - _startTime).TotalSeconds > IgnoreMouseDelaySecs)
					CloseApp();
			}
		}

		void CloseApp()
		{
			Application.Current.Shutdown();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_startTime = DateTime.Now;

			if (State.IsFullScreen)
				SetWindowFullScreen();

			_data.GetData(OnPriceTickUpdate, ReportException);
		}

		private void OnPriceTickUpdate(PriceTickDTO val)
		{
			DispatcherBeginInvoke(
				() =>
				{
					Chart.AddItem(val);
				});
		}

		private void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			_data.Dispose();
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
			DispatcherBeginInvoke(() => ReportExceptionDirectly(exc));
		}

		void DispatcherBeginInvoke(Action action)
		{
			Dispatcher.BeginInvoke(action, null);
		}

		private void ReportExceptionDirectly(Exception exc)
		{
			var msg = State.IsDebug ? exc.ToString() : exc.Message;
			MessageBox.Show(msg);
		}

		readonly Data _data = new Data();
		private DateTime _startTime;
		private const int IgnoreMouseDelaySecs = 3;
	}
}
