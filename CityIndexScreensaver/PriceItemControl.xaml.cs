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
	/// Interaction logic for PriceItemControl.xaml
	/// </summary>
	public partial class PriceItemControl : UserControl
	{
		public PriceItemControl()
		{
			InitializeComponent();
		}

		public void SetNewPrice(PriceDTO val)
		{
			DispatcherBeginInvoke(
				() =>
				{
					var prevVal = (PriceDTO)DataContext;
					if (prevVal != null)
					{
						var color = (prevVal.Price < val.Price) ? Colors.Red : Colors.Blue;
						BidPanel.Background = new SolidColorBrush(color);
						OfferPanel.Background = new SolidColorBrush(color);
					}
					DataContext = val;
				});
		}

		void DispatcherBeginInvoke(Action action)
		{
			Dispatcher.BeginInvoke(action, null);
		}
	}
}
