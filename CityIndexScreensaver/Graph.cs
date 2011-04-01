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
		public Graph()
		{
			View = new Canvas
			{
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
				Background = Brushes.Transparent
			};
		}

		public string Key;
		public readonly List<GraphItem> Items = new List<GraphItem>();
		public double ValueScale;
		public double MinValue;
		public double MaxValue;

		public double StartOffset;

		public Brush Brush { get; set; }
		public Canvas View { get; set; }

		public double ValueToView(double val)
		{
			return View.ActualHeight - val * ValueScale;
		}

		public void SetValueScale(double min, double max)
		{
			MinValue = min;
			MaxValue = max;

			//MinLabel.Content = graph.MinValue.ToString();
			//MaxLabel.Content = maxValue.ToString();

			var range = max - min;
			ValueScale = View.ActualHeight / range;
		}

		public Line CreateLine(double prevValue, double value, double prevOffset, double offset)
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

		public bool UpdateValueScale()
		{
			var minValue = Items.Min(x => x.Value);
			var maxValue = Items.Max(x => x.Value);

			if (minValue >= MinValue && maxValue <= MaxValue)
				return false;

			SetValueScale(minValue, maxValue);
			return true;
		}

		public void Shift(double step)
		{
			foreach (Line line in View.Children)
			{
				line.X1 += step;
				line.X2 += step;
			}
		}

		[Conditional("DEBUG")]
		public void Validate()
		{
			Debug.Assert(Items.Count == View.Children.Count + 1);
		}
	}
}
