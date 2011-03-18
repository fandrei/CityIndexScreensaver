using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
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
			if (_items.Count == 1) // need at least two points to make a line
				_items.Add(item);

			RebuildLines();
		}

		void RebuildLines()
		{
			Debug.WriteLine("RebuildLines: {0} items", _items.Count);

			Chart.Children.Clear();
			if (_items.Count == 0)
				return;

			UpdateValueScale();
			//UpdateTimeScale();

			double offset = _startOffset;
			double prevOffset = offset;
			double prevVal = 0;

			PriceTickDTO prevItem = null;
			int i = 0;
			foreach (var item in _items)
			{
				var val = (double)item.Price - _minValue;

				if (prevItem != null)
				{
					offset += GetDistance(prevItem, item);

					var line = CreateLine(prevVal, val, prevOffset, offset);
					line.Tag = i - 1;
					Chart.Children.Add(line);

					prevOffset = offset;
				}

				prevVal = val;
				prevItem = item;
				i++;
			}
		}

		private void UpdateValueScale()
		{
			_minValue = (double)_items.Min(x => x.Price);
			var maxPrice = (double)_items.Max(x => x.Price);
			var diffPrice = maxPrice - _minValue;

			if (diffPrice != 0)
				_valueScale = (Chart.ActualHeight - 1) / diffPrice;

			MinLabel.Content = _minValue.ToString();
			MaxLabel.Content = maxPrice.ToString();
		}

		private bool UpdateTimeScale()
		{
			var minTime = _items.First().TickDate;
			var maxTime = _items.Last().TickDate;
			var timeDiff = (maxTime - minTime).TotalSeconds;
			if (timeDiff == 0)
				return false;

			if (_items.Count >= MaxVisibleItemsCount)
				return false;

			_timeScale = Chart.ActualWidth / (timeDiff + _timeGap + _startOffset);
			RebuildLines();
			return true;
		}

		private double GetDistance(PriceTickDTO val1, PriceTickDTO val2)
		{
			return (val2.TickDate - val1.TickDate).TotalSeconds * _timeScale;
		}

		private Line CreateLine(double prevValue, double value, double prevOffset, double offset)
		{
			return new Line
			{
				X1 = prevOffset,
				Y1 = Chart.ActualHeight - prevValue * _valueScale,
				X2 = offset,
				Y2 = Chart.ActualHeight - value * _valueScale,
				Stroke = new SolidColorBrush { Color = Colors.LightGreen }
			};
		}

		private void InitTimer()
		{
			_timer.Interval = _timerPeriod;
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		public void TimerTick(object o, EventArgs sender)
		{
			if (Chart.Children.Count == 0)
				return;

			var maxOffset = Chart.Children.Cast<Line>().Last().X2;

			var offsetExceeded = maxOffset - (Chart.ActualWidth - _timeGap * _timeScale);
			if (offsetExceeded <= 0)
				return;
			if (offsetExceeded > 0 && UpdateTimeScale())
				return;

			var shiftSpeed = (offsetExceeded > 0) ? (offsetExceeded * 2 / _timeGap) : 0;
			var shiftSize = shiftSpeed * _timerPeriod.TotalSeconds;
			_startOffset -= shiftSize;

			Line lastInvisible = null;
			foreach (Line line in Chart.Children)
			{
				line.X1 -= shiftSize;
				line.X2 -= shiftSize;
				if (line.X2 < 0)
					lastInvisible = line;
			}

			if (lastInvisible != null)
			{
				_startOffset = lastInvisible.X2;
				Debug.Assert(_startOffset <= 0);
				var lastInvisibleIndex = (int)lastInvisible.Tag;
				_items.RemoveRange(0, lastInvisibleIndex + 1);

				RebuildLines();
			}
		}

		readonly List<PriceTickDTO> _items = new List<PriceTickDTO>();

		private double _valueScale = 1;
		private double _minValue = 0;

		private double _timeScale = 100;
		private double _startOffset;
		private double _timeGap = 1;
		private const int MaxVisibleItemsCount = 30;

		readonly DispatcherTimer _timer = new DispatcherTimer();
		private readonly TimeSpan _timerPeriod = TimeSpan.FromMilliseconds(50);
	}
}
