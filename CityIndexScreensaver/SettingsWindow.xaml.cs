using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using CIAPI.DTO;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Application.Current.DispatcherUnhandledException += OnUnhandledException;

			State.Data = new Data(ReportException);
			State.Data.GetMarketsList(markets => DispatcherBeginInvoke(() => RefreshMarketsView(markets)));

			DataContext = ApplicationSettings.Instance;
		}

		private void RefreshMarketsView(MarketDTO[] markets)
		{
			AllMarketsGrid.ItemsSource = markets;

			var marketNames = markets.ToDictionary(market => market.MarketId.ToString(), market => market.Name);

			_subscriptions.Clear();
			foreach (var id in ApplicationSettings.Instance.PricesToWatch)
			{
				string marketName;
				if (marketNames.TryGetValue(id, out marketName))
				{
					_subscriptions.Add(
						new Subscription
							{
								MarketId = id,
								SequenceNumber = _subscriptions.Count,
								Name = marketName
							});
				}
			}
			SubscriptionsGrid.ItemsSource = _subscriptions;
		}

		static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			try
			{
				ReportExceptionDirectly(args.Exception);
			}
			catch (Exception exc)
			{
				Debugger.Break();
			}

			args.Handled = true;
		}

		private void ReportException(Exception exc)
		{
			DispatcherBeginInvoke(() => ReportExceptionDirectly(exc));
		}

		void DispatcherBeginInvoke(Action action)
		{
			Dispatcher.BeginInvoke(action, null);
		}

		private static void ReportExceptionDirectly(Exception exc)
		{
			var msg = State.IsDebug ? exc.ToString() : exc.Message;
			MessageBox.Show(msg);
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			var subscribedIds = _subscriptions.Select(subscription => subscription.MarketId).ToArray();
			ApplicationSettings.Instance.PricesToWatch = subscribedIds;

			ApplicationSettings.Instance.Save();
			this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			// ApplicationSettings.Reload(); // not necessary, coz the app will terminate after settings window closed
			this.DialogResult = false;
		}

		private void ImageAdd_Click(object sender, MouseButtonEventArgs e)
		{
			var items = AllMarketsGrid.SelectedItems;
			foreach (MarketDTO market in items)
			{
				var newSubscription =
					new Subscription
					{
						MarketId = market.MarketId.ToString(),
						SequenceNumber = _subscriptions.Count,
						Name = market.Name
					};

				for (var i = 0; i < _subscriptions.Count; i++)
				{
					var cur = _subscriptions[i];
					if (cur.MarketId == newSubscription.MarketId)
					{
						_subscriptions.RemoveAt(i);
						break;
					}
				}
				_subscriptions.Add(newSubscription);
			}
		}

		private void ImageRemove_Click(object sender, MouseButtonEventArgs e)
		{
			var selected = SubscriptionsGrid.SelectedItems.Cast<Subscription>().ToList();
			foreach (var subscription in selected)
			{
				_subscriptions.Remove(subscription);
			}
		}

		class Subscription
		{
			public string MarketId { get; set; }
			public int SequenceNumber { get; set; }
			public string Name { get; set; }
		}

		private readonly ObservableCollection<Subscription> _subscriptions = new ObservableCollection<Subscription>();
	}
}
