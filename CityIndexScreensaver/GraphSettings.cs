using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CityIndexScreensaver
{
	class GraphSettings
	{
		public readonly Color[] GraphColors = new[] { Colors.Green, Colors.Red, Colors.Blue, Colors.Orange,
			Colors.Purple, Colors.Black, Colors.SteelBlue, Colors.DarkRed,  Colors.SlateGray, Colors.Orange, };

		// pixels per second
		public double TimeScale;

		public double MaxValue = 0.001;
		public double MinValue = 0.001;
		public const double ValueIncreaseStep = 0.0005;
		public double ValueRulerStep = ValueIncreaseStep * 2;

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
