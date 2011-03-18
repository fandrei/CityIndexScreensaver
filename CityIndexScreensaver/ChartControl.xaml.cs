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

			UpdateTimeScale();
			RebuildLines();
		}

		void RebuildLines()
		{
			//Debug.WriteLine("RebuildLines: {0} items", _items.Count);

			Graph.Children.Clear();
			if (_items.Count == 0)
				return;

			UpdateValueScale();

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
					Graph.Children.Add(line);

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
			var maxValue = (double)_items.Max(x => x.Price);
			var diff = maxValue - _minValue;

			if (diff != 0)
				_valueScale = (Graph.ActualHeight - 1) / diff;

			MinLabel.Content = _minValue.ToString();
			MaxLabel.Content = maxValue.ToString();
		}

		// update time scale to show as much items as possible, but not exceeding max items limit
		private void UpdateTimeScale()
		{
			if (_items.Count >= MaxVisibleItemsCount)
				return;

			var timeRange = GetTimeRange();
			if (timeRange == 0)
				return;

			_timeScale = (Graph.ActualWidth * OffsetMinThreshold - _startOffset) / timeRange;
			_timeScale /= ((double)MaxVisibleItemsCount / _items.Count);
			return;
		}

		private double GetTimeRange()
		{
			if (_items.Count == 0)
				return 0;
			var minTime = _items.First().TickDate;
			var maxTime = _items.Last().TickDate;
			var res = (maxTime - minTime).TotalSeconds;
			Debug.Assert(res >= 0);
			return res;
		}

		private double GetDistance(PriceTickDTO val1, PriceTickDTO val2)
		{
			return (val2.TickDate - val1.TickDate).TotalSeconds * _timeScale;
		}

		private Line CreateLine(double prevValue, double value, double prevOffset, double offset)
		{
			var res = new Line
			{
				X1 = prevOffset,
				X2 = offset,
				Y1 = Graph.ActualHeight - prevValue * _valueScale - 1,
				Y2 = Graph.ActualHeight - value * _valueScale - 1,
				Stroke = new SolidColorBrush { Color = Colors.LightGreen }
			};
			return res;
		}

		private void InitTimer()
		{
			_timer.Interval = _timerPeriod;
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		public void TimerTick(object o, EventArgs sender)
		{
			var timeRange = GetTimeRange();
			if (timeRange == 0)
				return;

			var maxOffset = Graph.Children.Cast<Line>().Last().X2;

			var shiftSpeed = (Graph.ActualWidth * OffsetMaxThreshold - _startOffset) / timeRange;
			var correctionRatio = (maxOffset - Graph.ActualWidth * OffsetMinThreshold) /
				(Graph.ActualWidth * OffsetMaxThreshold - Graph.ActualWidth * OffsetMinThreshold);
			if (correctionRatio < 0)
				correctionRatio = 0;
			shiftSpeed *= correctionRatio;
			Debug.Assert(shiftSpeed >= 0);
			Debug.WriteLine("Shift speed: {0}", shiftSpeed);

			var shiftStep = shiftSpeed * _timerPeriod.TotalSeconds;
			_startOffset -= shiftStep;
			Debug.Assert(_startOffset <= 0);

			Line lastInvisible = null;
			foreach (Line line in Graph.Children)
			{
				line.X1 -= shiftStep;
				line.X2 -= shiftStep;
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
		private double _minValue;

		private double _timeScale = 10; // pixels per second
		private double _startOffset;

		// fractions of the total width
		private const double OffsetMaxThreshold = 0.9;
		private const double OffsetMinThreshold = 0.8;

		private const int MaxVisibleItemsCount = 30;

		readonly DispatcherTimer _timer = new DispatcherTimer();
		private readonly TimeSpan _timerPeriod = TimeSpan.FromMilliseconds(50);
	}
}
