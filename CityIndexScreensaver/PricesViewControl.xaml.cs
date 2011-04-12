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
using System.Windows.Threading;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for PricesViewControl.xaml
	/// </summary>
	public partial class PricesViewControl : UserControl
	{
		public PricesViewControl()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			InitTimer();
		}

		private void InitTimer()
		{
			_timer.Interval = TimeSpan.FromSeconds(3);
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			var i = (PricesGrid.SelectedIndex + 1) % PricesGrid.Items.Count;
			PricesGrid.SelectedIndex = i;
		}

		private readonly DispatcherTimer _timer = new DispatcherTimer();
	}
}
