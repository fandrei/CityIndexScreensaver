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
	/// Interaction logic for ChartControl.xaml
	/// </summary>
	public partial class ChartControl : UserControl
	{
		public ChartControl()
		{
			InitializeComponent();
		}

		public void AddItem(PriceTickDTO item)
		{
			_items.Add(item);
			DrawItems();
		}

		void DrawItems()
		{
			Chart.Children.Clear();
			if (_items.Count == 0)
				return;

			var minPrice = (double)_items.Min(x => x.Price);
			var maxPrice = (double)_items.Max(x => x.Price);
			if (maxPrice * _heightScale > Chart.ActualHeight)
			{
				var initialScale = (_heightScale == 1);
				_heightScale = Chart.ActualHeight / maxPrice;
				if (initialScale)
					_heightScale /= 2;
			}

			double maxTimeDistance = 0;
			PriceTickDTO prevItem = null;
			foreach (var item in _items)
			{
				if (prevItem != null)
				{
					var distance = GetDistance(prevItem, item);
					maxTimeDistance = Math.Max(maxTimeDistance, distance);
				}
				prevItem = item;
			}

			double offset = 0;
			double prevOffset = offset;
			double prevPrice = 0;

			prevItem = null;
			foreach (var item in _items)
			{
				var price = (double)item.Price;
				if (prevPrice == 0)
					prevPrice = price;

				if (prevItem != null)
					offset += GetDistance(prevItem, item);

				var line = CreateLine(prevPrice, price, prevOffset, offset);
				Chart.Children.Add(line);

				prevOffset = offset;
				prevPrice = price;
				prevItem = item;
			}

			var offsetExceeded = offset - Chart.ActualWidth;
			if (offsetExceeded > 0)
			{
				offset = 0;
				prevItem = null;
				var i = 0;
				while (offset < offsetExceeded)
				{
					var item = _items[i++];
					if (prevItem != null)
						offset += GetDistance(prevItem, item);
					prevItem = item;
				}
				_items.RemoveRange(0, i);
			}
		}

		private double GetDistance(PriceTickDTO val1, PriceTickDTO val2)
		{
			return (val2.TickDate - val1.TickDate).TotalSeconds * _timeScale;
		}

		private Line CreateLine(double prevPrice, double price, double prevOffset, double offset)
		{
			return new Line
			{
				X1 = prevOffset,
				Y1 = Chart.ActualHeight - prevPrice * _heightScale,
				X2 = offset,
				Y2 = Chart.ActualHeight - price * _heightScale,
				Stroke = new SolidColorBrush { Color = Colors.LightGreen }
			};
		}

		private double _heightScale = 1;
		private double _timeScale = 100;

		readonly List<PriceTickDTO> _items = new List<PriceTickDTO>();
	}
}
