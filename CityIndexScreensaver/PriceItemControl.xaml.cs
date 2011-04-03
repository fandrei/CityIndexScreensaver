using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CIAPI.DTO;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for PriceItemControl.xaml
	/// </summary>
	public partial class PriceItemControl : UserControl
	{
		public PriceItemControl()
		{
			InitializeComponent();
			_brushIncreasing = (Brush)FindResource("PanelBrushIncreasing");
			_brushDecreasing = (Brush)FindResource("PanelBrushDecreasing");
		}

		private readonly Brush _brushIncreasing;
		private readonly Brush _brushDecreasing;

		private Color _color;
		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				ColorMark.Background = new SolidColorBrush(_color);
			}
		}

		public void SetNewPrice(PriceDTO val)
		{
			var prevVal = (PriceDTO)DataContext;
			if (prevVal != null)
			{
				var brush = (prevVal.Price < val.Price) ? _brushIncreasing : _brushDecreasing;
				ChangePanel.Background = brush;
			}
			DataContext = val;
		}
	}
}
