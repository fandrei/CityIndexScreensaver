using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CityIndexScreensaver
{
	public class PriceInfo : DependencyObject
	{
		public int MarketId { get; set; }
		public SolidColorBrush Color { get; set; }

		public static readonly DependencyProperty MarketNameProperty =
			DependencyProperty.Register("MarketName", typeof(string),
			typeof(PriceInfo), new FrameworkPropertyMetadata(""));

		public string MarketName
		{
			get { return (string)GetValue(MarketNameProperty); }
			set { SetValue(MarketNameProperty, value); }
		}

		public static readonly DependencyProperty ChangeProperty =
			DependencyProperty.Register("Change", typeof(decimal),
			typeof(PriceInfo), new FrameworkPropertyMetadata(decimal.Zero));

		public decimal Change
		{
			get { return (decimal)GetValue(ChangeProperty); }
			set
			{
				SetValue(ChangeProperty, value);
				ChangeSign = Math.Sign(value);
			}
		}

		public static readonly DependencyProperty ChangeSignProperty =
			DependencyProperty.Register("ChangeSign", typeof(decimal),
			typeof(PriceInfo), new FrameworkPropertyMetadata(decimal.Zero));

		public decimal ChangeSign
		{
			get { return (decimal)GetValue(ChangeSignProperty); }
			set { SetValue(ChangeSignProperty, value); }
		}

		public static readonly DependencyProperty PriceProperty =
			DependencyProperty.Register("Price", typeof(decimal),
			typeof(PriceInfo), new FrameworkPropertyMetadata(decimal.Zero));

		public decimal Price
		{
			get { return (decimal)GetValue(PriceProperty); }
			set { SetValue(PriceProperty, value); }
		}
	}
}
