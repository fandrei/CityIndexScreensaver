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
			var topics = new[] { "PRICES.PRICE.99498", "PRICES.PRICE.99500", "PRICES.PRICE.99502", 
				"PRICES.PRICE.99504", };

			foreach (var topic in topics)
			{
				var control = new PriceItemControl();
				control.CaptionLabel.Content = topic;

				State.Data.SubscribePrices(topic, control.SetNewPrice);

				PricesPanel.Children.Add(control);
			}
		}

	}
}
