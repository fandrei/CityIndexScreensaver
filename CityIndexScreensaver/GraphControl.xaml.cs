using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for GraphControl.xaml
	/// </summary>
	public partial class GraphControl : UserControl
	{
		public GraphControl()
		{
			InitializeComponent();
			InitTimer();
		}

		public void AddItem(string key, GraphItem item)
		{
			Graph graph;
			if (!_graphs.TryGetValue(key, out graph))
			{
				graph = new Graph(_settings) { Key = key };
				var colors = _settings.MyColors;
				graph.Brush = new SolidColorBrush { Color = colors[_graphs.Count % colors.Length] };
				GraphBackground.Children.Add(graph.View);

				_graphs.Add(key, graph);
			}

			var valFraction = graph.ValueToFraction(item.Value);
			if (_settings.UpdateValueScale(valFraction))
				RebuildLines();

			graph.Add(item);
		}

		private void GraphBackground_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateTimeScale();
			RebuildLines();
		}

		void RebuildLines()
		{
			foreach (var pair in _graphs)
			{
				var graph = pair.Value;
				graph.RebuildLines();
			}
		}

		private void UpdateTimeScale()
		{
			_settings.TimeScale = GraphBackground.ActualWidth * OffsetOptimalThreshold / _settings.VisiblePeriodSecs;
		}

		private void InitTimer()
		{
			_timer.Interval = _timerPeriod;
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		void TimerTick(object o, EventArgs sender)
		{
			Shift();
		}

		void Shift()
		{
			// correction ratio should be 1 at optimal offset, 0 at minimal and grow to 2x at max offset
			var maxOffset = GetMaxOffset();
			var correctionRatio = (maxOffset - GraphBackground.ActualWidth * OffsetMinThreshold) /
				(GraphBackground.ActualWidth * (OffsetMaxThreshold - OffsetOptimalThreshold));
			if (correctionRatio < 0)
				return;

			var shiftSpeed = _settings.TimeScale * correctionRatio;
			Debug.Assert(shiftSpeed >= 0);

			var shiftStep = shiftSpeed * _timerPeriod.TotalSeconds;

			foreach (var pair in _graphs)
			{
				var graph = pair.Value;
				graph.Shift(shiftStep);
			}
		}

		private double GetMaxOffset()
		{
			double res = 0;

			foreach (var pair in _graphs)
			{
				var graph = pair.Value;
				if (graph.Items.Count == 0)
					continue;
				var lastItem = (Line)graph.View.Children[graph.View.Children.Count - 1];
				var cur = lastItem.X2;
				if (cur > res)
					res = cur;
			}

			return res;
		}

		// fraction of the total width
		const double OffsetMaxThreshold = 0.9;
		const double OffsetMinThreshold = 0.8;

		static double OffsetOptimalThreshold
		{
			get { return OffsetMinThreshold + (OffsetMaxThreshold - OffsetMinThreshold) / 2; }
		}

		private readonly TimeSpan _timerPeriod = TimeSpan.FromMilliseconds(50);
		private readonly DispatcherTimer _timer = new DispatcherTimer();
		private readonly Dictionary<string, Graph> _graphs = new Dictionary<string, Graph>();
		private readonly GraphSettings _settings = new GraphSettings();
	}
}
