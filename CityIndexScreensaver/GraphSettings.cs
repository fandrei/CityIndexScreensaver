using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CityIndexScreensaver
{
	class GraphSettings
	{
		public int VisiblePeriodSecs = 60;

		public readonly Color[] GraphColors = new[] { Colors.LightGreen, Colors.Red, Colors.Blue, Colors.Coral,
			Colors.Cyan, Colors.Pink, Colors.SeaGreen, Colors.SteelBlue };

		// pixels per second
		public double TimeScale;

		public double MaxValue = 0.05;
		public double MinValue = 0.05;
		public const double ValueIncreaseStep = 0.01;
		public double ValueRulerStep = 0.02;

		public bool UpdateValueScale(double val)
		{
			if (val < -MinValue)
			{
				MinValue += ValueIncreaseStep;
				return true;
			}
			if (val > MaxValue)
			{
				MaxValue += ValueIncreaseStep;
				return true;
			}
			return false;
		}

		public double GetAdjustedValue(double val)
		{
			return ((MaxValue - val) / (MinValue + MaxValue));
		}
	}
}
