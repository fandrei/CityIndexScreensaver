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
			InitTimer();
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

			MinLabel.Content = minPrice.ToString();
			MaxLabel.Content = maxPrice.ToString();

			double offset = _startOffset;
			double prevOffset = offset;
			double prevPrice = 0;

			PriceTickDTO prevItem = null;
			foreach (var item in _items)
			{
				var price = (double)item.Price - minPrice + 1;
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

		private void InitTimer()
		{
			_timer.Interval = TimeSpan.FromMilliseconds(TimerPeriodMsecs);
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		public void TimerTick(object o, EventArgs sender)
		{
			if (Chart.Children.Count == 0)
				return;

			double maxOffset = 0;
			foreach (Line line in Chart.Children)
			{
				maxOffset = Math.Max(maxOffset, line.X2);
			}

			var offsetExceeded = maxOffset - (Chart.ActualWidth - _timeGap * _timeScale);
			var shiftSpeed = offsetExceeded > 0 ? offsetExceeded * 2 : 0;

			var shiftSize = shiftSpeed * TimerPeriodMsecs / 1000;
			_startOffset -= shiftSize;

			foreach (Line line in Chart.Children)
			{
				line.X1 -= shiftSize;
				line.X2 -= shiftSize;
			}
		}

		readonly List<PriceTickDTO> _items = new List<PriceTickDTO>();

		private double _heightScale = 1;
		private double _timeScale = 100;
		private double _startOffset;
		private double _timeGap = 1;

		readonly DispatcherTimer _timer = new DispatcherTimer();
		private const int TimerPeriodMsecs = 50;
	}
}
