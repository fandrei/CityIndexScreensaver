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
	}
}
