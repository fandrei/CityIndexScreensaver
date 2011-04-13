using System;
using System.Collections.Generic;
using System.ComponentModel;
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
			Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
			if (!DesignerProperties.GetIsInDesignMode(this))
				InitTimer();
		}

		void Dispatcher_ShutdownStarted(object sender, EventArgs e)
		{
			if (_timer != null)
				_timer.Stop();
		}

		public void AddItem(string key, GraphItem item)
		{
			var graph = GetGraph(key);

			var valFraction = graph.ValueToFraction(item.Value);
			if (_settings.UpdateValueScale(valFraction))
				RebuildLines();

			graph.Add(item);
		}

		public void SetCurrentVisible(string key)
		{
			var graph = GetGraph(key);
			graph.Visible = true;

			foreach (var pair in _graphs)
			{
				var cur = pair.Value;
				if (!ReferenceEquals(graph, cur))
					cur.Visible = false;
			}
		}

		public Color GetGraphColor(string key)
		{
			var graph = GetGraph(key);
			return graph.Brush.Color;
		}

		public void SetGraphBrush(string key, SolidColorBrush brush)
		{
			var graph = GetGraph(key);
			graph.Brush = brush;
		}

		Graph GetGraph(string key)
		{
			Graph graph;
			if (!_graphs.TryGetValue(key, out graph))
			{
				graph = new Graph(_settings) { Key = key };
				var colors = _settings.GraphColors;
				graph.Brush = new SolidColorBrush { Color = colors[_graphs.Count % colors.Length] };
				GraphBackground.Children.Add(graph.View);

				_graphs.Add(key, graph);
			}
			return graph;
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

			DrawValueRulers();
		}

		void DrawValueRulers()
		{
			GridRulersCanvas.Children.Clear();

			var valueRulerStep = _settings.ValueRulerStep;
			for (var cur = -Math.Floor(_settings.MinValue / valueRulerStep) * valueRulerStep;
				cur <= _settings.MaxValue; cur += valueRulerStep)
			{
				var height = (GraphBackground.ActualHeight - 1) * _settings.GetAdjustedValue(cur);

				var line = new Line
				{
					X1 = 0,
					X2 = GraphBackground.ActualWidth,
					Y1 = height,
					Y2 = height,
					Stroke = Brushes.DarkGray
				};
				GridRulersCanvas.Children.Add(line);

				var labelText = string.Format("{0}%", Math.Round(cur*100, 2));
				var label = new Label {Content = labelText, Foreground = Brushes.Black};
				GridRulersCanvas.Children.Add(label);

				label.UpdateLayout();
				if (height > GraphBackground.ActualHeight - label.ActualHeight)
					height -= label.ActualHeight;

				Canvas.SetTop(label, height);
				Canvas.SetRight(label, 0);
			}
		}

		private void UpdateTimeScale()
		{
			_settings.TimeScale = GraphBackground.ActualWidth * OffsetOptimalThreshold / 
				ApplicationSettings.Instance.GraphPeriodSecs;
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
				if (graph.View.Children.Count == 0)
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
