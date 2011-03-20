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
			_brushIncreasing = (Brush)FindResource("PanelBrushIncreasing");
			_brushDecreasing = (Brush)FindResource("PanelBrushDecreasing");
		}

		private readonly Brush _brushIncreasing;
		private readonly Brush _brushDecreasing;

		public void SetNewPrice(PriceDTO val)
		{
			DispatcherBeginInvoke(
				() =>
				{
					var prevVal = (PriceDTO)DataContext;
					if (prevVal != null)
					{
						var brush = (prevVal.Price < val.Price) ? _brushIncreasing : _brushDecreasing;
						BidPanel.Background = brush;
						OfferPanel.Background = brush;
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
