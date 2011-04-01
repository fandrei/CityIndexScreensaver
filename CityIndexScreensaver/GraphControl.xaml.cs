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

		private void GraphBackground_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateTimeScale();
			RebuildLines();
		}

		public void AddItem(string key, GraphItem item)
		{
			Graph graph;
			if (!_graphs.TryGetValue(key, out graph))
			{
				graph = new Graph { Key = key };
				graph.Brush = new SolidColorBrush { Color = _myColors[_graphs.Count % _myColors.Length] };
				GraphBackground.Children.Add(graph.View);

				_graphs.Add(key, graph);
			}

			var items = graph.Items;
			items.Add(item);
			if (items.Count == 1) // need at least two points to make a line
				items.Add(item);

			if (graph.ValueScale == 0)
				SetInitialValueScale(graph, item);

			if (graph.UpdateValueScale())
				RebuildLines();
			else
				AddNewLine(graph, graph.Items.Count - 1);

			graph.Validate();
		}

		void AddNewLine(Graph graph, int i)
		{
			Debug.Assert(graph.Items.Count >= 2 && i > 0);

			var newItem = graph.Items[i];
			var prev = graph.Items[i - 1];
			var line = CreateLine(graph, prev, newItem);
			graph.View.Children.Add(line);
		}

		Line CreateLine(Graph graph, GraphItem prev, GraphItem newItem)
		{
			var first = graph.Items.First();

			var val = newItem.Value - graph.MinValue;
			var prevVal = prev.Value - graph.MinValue;

			var offset = graph.StartOffset + GetDistance(first, newItem);
			var prevOffset = graph.StartOffset + GetDistance(first, prev);

			var line = graph.CreateLine(prevVal, val, prevOffset, offset);

			return line;
		}

		private double GetDistance(GraphItem val1, GraphItem val2)
		{
			return (val2.Time - val1.Time).TotalSeconds * _timeScale;
		}

		void RebuildLines(Graph graph)
		{
			graph.View.Children.Clear();
			for (var i = 1; i < graph.Items.Count; i++)
				AddNewLine(graph, i);
		}

		void RebuildLines()
		{
			foreach (var pair in _graphs)
			{
				RebuildLines(pair.Value);
			}
		}

		private static void SetInitialValueScale(Graph graph, GraphItem item)
		{
			var value = item.Value;
			var diff = value * InitialValueRange / 2;
			var min = value - diff;
			var max = value + diff;

			graph.SetValueScale(min, max);
		}

		private void UpdateTimeScale()
		{
			_timeScale = GraphBackground.ActualWidth * OffsetOptimalThreshold / VisiblePeriodSecs;
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
			var maxOffset = GetMaxOffset();
			var correctionRatio = (maxOffset - GraphBackground.ActualWidth * OffsetMinThreshold) /
				(GraphBackground.ActualWidth * (OffsetMaxThreshold - OffsetOptimalThreshold));
			if (correctionRatio < 0)
				return;

			var shiftSpeed = _timeScale * correctionRatio;
			Debug.Assert(shiftSpeed >= 0);
			//Debug.WriteLine("Shift speed: {0}", shiftSpeed);

			var shiftStep = shiftSpeed * _timerPeriod.TotalSeconds;

			foreach (var pair in _graphs)
			{
				var graph = pair.Value;
				Shift(graph, shiftStep);
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

		private void Shift(Graph graph, double shiftStep)
		{
			graph.Shift(-shiftStep);

			var lastInvisibleIndex = -1;
			for (var i = 0; i < graph.View.Children.Count; i++)
			{
				var line = (Line)graph.View.Children[i];
				if (line.X2 < 0)
					lastInvisibleIndex = i;
			}

			if (lastInvisibleIndex != -1)
			{
				var deleteCount = lastInvisibleIndex + 1;
				graph.Items.RemoveRange(0, deleteCount);
				graph.View.Children.RemoveRange(0, deleteCount);
				graph.Validate();
			}

			var firstVisible = (Line) graph.View.Children[0];
			graph.StartOffset = firstVisible.X1;
		}

		#region Visual Settings

		private const int VisiblePeriodSecs = 60;

		// fraction of the total width
		private const double OffsetMaxThreshold = 0.9;
		private const double OffsetMinThreshold = 0.8;

		private static double OffsetOptimalThreshold
		{
			get { return OffsetMinThreshold + (OffsetMaxThreshold - OffsetMinThreshold) / 2; }
		}

		// fraction of the total value
		private const double InitialValueRange = 0.1;

		readonly Color[] _myColors = new[] { Colors.LightGreen, Colors.Red, Colors.Blue, Colors.Coral,
			Colors.Cyan, Colors.Pink, Colors.SeaGreen, Colors.SteelBlue };

		private readonly TimeSpan _timerPeriod = TimeSpan.FromMilliseconds(50);

		#endregion

		#region State

		private double _timeScale; // pixels per second

		private readonly Dictionary<string, Graph> _graphs = new Dictionary<string, Graph>();

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		#endregion
	}
}
