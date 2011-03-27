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
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (State.Data == null)
				return;

			var prices = ApplicationSettings.Instance.PricesToWatch;
			State.Data.GetMarketsList(
				markets =>
					{
						var marketNames = markets.ToDictionary(market => market.MarketId, market => market.Name);

						Dispatcher.BeginInvoke(
							new Action(() =>
								{
									foreach (var id in prices)
									{
										var control = new PriceItemControl();

										string marketName;
										if (marketNames.TryGetValue(id, out marketName))
										{
											control.CaptionLabel.Content = marketName;
											State.Data.SubscribePrices(id, control.SetNewPrice);
											PricesPanel.Children.Add(control);
										}
									}
								}));
					});

		}
	}
}
