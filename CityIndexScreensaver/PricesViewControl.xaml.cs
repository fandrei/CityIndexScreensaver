﻿using System;
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

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for PricesViewControl.xaml
	/// </summary>
	public partial class PricesViewControl : UserControl
	{
		public PricesViewControl()
		{
			InitializeComponent();

			_brushIncreasing = (Brush)FindResource("PanelBrushIncreasing");
			_brushDecreasing = (Brush)FindResource("PanelBrushDecreasing");
		}

		private readonly Brush _brushIncreasing;
		private readonly Brush _brushDecreasing;
	}
}
