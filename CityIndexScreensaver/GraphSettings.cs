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

		public readonly Color[] MyColors = new[] { Colors.LightGreen, Colors.Red, Colors.Blue, Colors.Coral,
			Colors.Cyan, Colors.Pink, Colors.SeaGreen, Colors.SteelBlue };

		// pixels per second
		public double TimeScale;

		public double MaxValueFraction = 0.01;
		public double MinValueFraction = 0.01;
		public const double ValueGridStep = 0.01;

		public bool UpdateValueScale(double val)
		{
			if (val < -MinValueFraction)
			{
				MinValueFraction += ValueGridStep;
				return true;
			}
			if (val > MaxValueFraction)
			{
				MaxValueFraction += ValueGridStep;
				return true;
			}
			return false;
		}

		public double GetAdjustedValue(double val)
		{
			return ((MaxValueFraction - val) / (MinValueFraction + MaxValueFraction));
		}
	}
}
