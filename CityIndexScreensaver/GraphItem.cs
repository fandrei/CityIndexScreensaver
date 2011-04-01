using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CityIndexScreensaver
{
	public class GraphItem
	{
		public double Value { get; set; }
		public DateTime Time { get; set; }

		public override string ToString()
		{
			var res = string.Format("{0} -> {1}", Time, Value);
			return res;
		}
	}
}
