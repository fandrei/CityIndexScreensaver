using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using CityIndexScreensaver.Utils;

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
			Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

			this.HideMinimizeAndMaximizeButtons();

			State.Data = new Data(ReportException);
			State.Data.GetMarketsList(RefreshMarketsView);

			DataContext = ApplicationSettings.Instance;
		}

		private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
		{
			State.Data.Dispose();
		}

		private void RefreshMarketsView(MarketDTO[] markets)
		{
			_marketsView = CollectionViewSource.GetDefaultView(markets);
			AllMarketsGrid.ItemsSource = _marketsView;

			_subscriptions.Clear();
			foreach (var market in ApplicationSettings.Instance.PricesToWatch)
			{
				_subscriptions.Add(market);
			}

			SubscriptionsGrid.ItemsSource = _subscriptions;
			_subscriptions.CollectionChanged += SubscriptionsCollectionChanged;
		}

		void SubscriptionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				SubscriptionsGrid.SelectedIndex = e.OldStartingIndex;
			}
		}

		void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			try
			{
				ReportException(args.Exception);
			}
			catch (Exception exc)
			{
				Debugger.Break();
			}

			args.Handled = true;
		}

		private void ReportException(Exception exc)
		{
			var msg = State.IsDebug ? exc.ToString() : exc.Message;
			MessageBox.Show(this, msg, Const.AppName);
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			ApplicationSettings.Instance.PricesToWatch = _subscriptions.ToArray();

			if (!string.IsNullOrEmpty(PasswordEdit.Password))
				ApplicationSettings.Instance.Password = PasswordEdit.Password;

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
				var newSubscription = new MarketDTO { MarketId = market.MarketId, Name = market.Name };

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
			var cell = (FrameworkElement)e.OriginalSource;
			var item = (MarketDTO)cell.DataContext;

			var i = _subscriptions.IndexOf(item);
			_subscriptions.RemoveAt(i);
			SubscriptionsGrid.SelectedIndex = i;
		}

		private void SubscriptionsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_draggingItemIndex != -1)
				return;

			var cell = e.OriginalSource as FrameworkElement;
			if (cell == null)
				return;

			var source = cell.DataContext as MarketDTO;
			if (source == null)
				return;

			_draggingItemIndex = _subscriptions.IndexOf(source);
			//Debug.WriteLine("Start dragging: {0}", _draggingItemIndex);
		}

		private void SubscriptionsGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			//Debug.WriteLine("Stop dragging");
			_draggingItemIndex = -1;
		}

		// DataGrid mouse move events are suppressed, when left mouse button is pressed
		// so, subscribe to mouse move events of the parent control instead
		private void SubscriptionsGrid_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (_draggingItemIndex == -1)
				return;

			var cell = e.OriginalSource as FrameworkElement;
			if (cell == null)
				return;

			var current = cell.DataContext as MarketDTO;
			if (current == null)
				return;

			var curIndex = _subscriptions.IndexOf(current);
			if (curIndex != _draggingItemIndex)
			{
				//Debug.WriteLine("{0}->{1}", _draggingItemIndex, curIndex);
				_subscriptions.Move(_draggingItemIndex, curIndex);
				_draggingItemIndex = curIndex;
			}
			SubscriptionsGrid.SelectedIndex = curIndex;
		}

		private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			_marketsView.Filter =
				val =>
				{
					var market = (MarketDTO)val;
					var res = market.Name.ToLower().Contains(FilterTextBox.Text.ToLower());
					return res;
				};
		}

		private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				FilterTextBox.Text = "";
				e.Handled = true;
			}
		}

		private void TestLogin_Click(object sender, RoutedEventArgs e)
		{
			TestLogin.IsEnabled = false;
			ApplicationSettings.Instance.Password = PasswordEdit.Password;

			State.Data.Logout(
				() => State.Data.EnsureConnection(
					() =>
					{
						TestLogin.IsEnabled = true;
						MessageBox.Show(this, "Connected successfully", Const.AppName);
					},
					() =>
					{
						TestLogin.IsEnabled = true;
					}));
		}

		private ICollectionView _marketsView;
		private readonly ObservableCollection<MarketDTO> _subscriptions = new ObservableCollection<MarketDTO>();

		private int _draggingItemIndex = -1;
	}
}
