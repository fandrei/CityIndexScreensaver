using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CityIndexScreensaver
{
	public partial class TickerControl : UserControl
	{
		public TickerControl()
		{
			InitializeComponent();
		}

		private DispatcherTimer _timer;

		private void NewsGrid_Loaded(object sender, RoutedEventArgs e)
		{
			NewsGrid.SelectedItem = null;
		}

		private void InitTimer()
		{
			_timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(20)};
			_timer.Tick += TimerTick;
			_timer.Start();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			GridTranslateTransform.Y -= 1;
			if (GridTranslateTransform.Y + NewsGrid.ActualHeight < 0)
				GridTranslateTransform.Y = ControlRoot.ActualHeight;
		}

		private bool _animationStarted;

		private void NewsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (DataContext == null)
				return;

			if (NewsGrid.ActualHeight < ControlRoot.ActualHeight)
				return;

			if (_animationStarted)
				return;

			if (!_animationStarted)
			{
				InitTimer();
				_animationStarted = true;
			}
		}
	}
}
