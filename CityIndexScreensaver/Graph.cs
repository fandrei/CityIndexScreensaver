using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CityIndexScreensaver
{
	class Graph
	{
		public Graph(GraphSettings settings)
		{
			_settings = settings;

			View = new Canvas
			{
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
				Background = Brushes.Transparent
			};
		}

		private readonly GraphSettings _settings;

		public string Key;
		public readonly List<GraphItem> Items = new List<GraphItem>();
		public double ValueScale;
		public double MinValue;
		public double MaxValue;

		public double StartOffset;

		public Brush Brush { get; set; }
		public Canvas View { get; set; }

		public void Add(GraphItem item)
		{
			Items.Add(item);
			if (Items.Count == 1) // need at least two points to make a line
				Items.Add(item);

			if (ValueScale == 0)
				SetInitialValueScale(item);

			if (UpdateValueScale())
				RebuildLines();
			else
				AddNewLine(Items.Count - 1);

			Validate();
		}

		private double ValueToView(double val)
		{
			return View.ActualHeight - val * ValueScale;
		}

		private void SetValueScale(double min, double max)
		{
			MinValue = min;
			MaxValue = max;

			//MinLabel.Content = graph.MinValue.ToString();
			//MaxLabel.Content = maxValue.ToString();

			var range = max - min;
			ValueScale = View.ActualHeight / range;
		}

		private Line CreateLine(double prevValue, double value, double prevOffset, double offset)
		{
			var res = new Line
			{
				X1 = prevOffset,
				X2 = offset,
				Y1 = ValueToView(prevValue),
				Y2 = ValueToView(value),
				Stroke = Brush
			};
			return res;
		}

		private bool UpdateValueScale()
		{
			var minValue = Items.Min(x => x.Value);
			var maxValue = Items.Max(x => x.Value);

			if (minValue >= MinValue && maxValue <= MaxValue)
				return false;

			SetValueScale(minValue, maxValue);
			return true;
		}

		private void SetInitialValueScale(GraphItem item)
		{
			var value = item.Value;
			var diff = value * InitialValueRange / 2;
			var min = value - diff;
			var max = value + diff;

			SetValueScale(min, max);
		}

		void AddNewLine(int i)
		{
			Debug.Assert(Items.Count >= 2 && i > 0);

			var newItem = Items[i];
			var prev = Items[i - 1];
			var line = CreateLine(prev, newItem);
			View.Children.Add(line);
		}

		Line CreateLine(GraphItem prev, GraphItem newItem)
		{
			var first = Items.First();

			var val = newItem.Value - MinValue;
			var prevVal = prev.Value - MinValue;

			var offset = StartOffset + GetDistance(first, newItem);
			var prevOffset = StartOffset + GetDistance(first, prev);

			var line = CreateLine(prevVal, val, prevOffset, offset);

			return line;
		}

		private double GetDistance(GraphItem val1, GraphItem val2)
		{
			return (val2.Time - val1.Time).TotalSeconds * _settings.TimeScale;
		}

		public void RebuildLines()
		{
			View.Children.Clear();
			for (var i = 1; i < Items.Count; i++)
				AddNewLine(i);
		}

		public void Shift(double step)
		{
			foreach (Line line in View.Children)
			{
				line.X1 -= step;
				line.X2 -= step;
			}

			var lastInvisibleIndex = -1;
			for (var i = 0; i < View.Children.Count; i++)
			{
				var line = (Line)View.Children[i];
				if (line.X2 < 0)
					lastInvisibleIndex = i;
			}

			if (lastInvisibleIndex != -1)
			{
				var deleteCount = lastInvisibleIndex + 1;
				Items.RemoveRange(0, deleteCount);
				View.Children.RemoveRange(0, deleteCount);
				Validate();
			}

			var firstVisible = (Line)View.Children[0];
			StartOffset = firstVisible.X1;
		}

		[Conditional("DEBUG")]
		public void Validate()
		{
			Debug.Assert(Items.Count == View.Children.Count + 1);
		}

		// fraction of the total value
		private const double InitialValueRange = 0.1;
	}
}
