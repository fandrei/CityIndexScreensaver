using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace CityIndexScreensaver
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{
				var args = e.Args;

				if (args.Length == 0 || args.Length > 2)
				{
					Shutdown();
					return;
				}

				var firstArg = args[0].ToLower();
				var secondArg = "";
				var parts = firstArg.Split(':');

				if (parts.Length > 2)
					Shutdown();
				else if (parts.Length == 2)
				{
					firstArg = parts[0];
					secondArg = parts[1];
				}
				else if (parts.Length == 1)
				{
					if (args.Length == 2)
						secondArg = args[1];
				}

				ProcessArgs(firstArg, secondArg);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message);
			}
		}

		private void ProcessArgs(string firstArg, string secondArg)
		{
			// config
			if (firstArg == "/c")
			{
				//var handle = int.Parse(secondArg);
				ShowSettingsWindow();
				Shutdown();
				return;
			}

			// show in full-screen mode
			if (firstArg == "/s")
			{
				State.IsFullScreen = true;
				return;
			}

			// show in debug mode
			if (firstArg == "/d")
			{
				State.IsDebug = true;
				return;
			}

			// windowed preview
			if (firstArg == "/p")
			{
				Shutdown();
				return;
			}

			Shutdown();
		}

		private void ShowSettingsWindow()
		{
			var window = new SettingsWindow();
			window.DataContext = ApplicationSettings.Instance;
			window.ShowDialog();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{

		}
	}
}
