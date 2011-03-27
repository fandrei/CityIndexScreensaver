using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

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

				// args can be represented as /c:1444226 or /c 1444226
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

				firstArg = firstArg.ToLower();
				secondArg = secondArg.ToLower();

				ProcessArgs(firstArg, secondArg);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, Const.AppName);
				Shutdown();
			}
		}

		private void ProcessArgs(string firstArg, string secondArg)
		{
			// config
			if (firstArg == "/c")
			{
				var hParent = !string.IsNullOrEmpty(secondArg) ? (IntPtr)int.Parse(secondArg) : IntPtr.Zero;
				ShowSettingsWindow(hParent);
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

		private void ShowSettingsWindow(IntPtr hParent)
		{
			var window = new SettingsWindow();
			var helper = new WindowInteropHelper(window) { Owner = hParent };
			window.ShowDialog();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
		}
	}
}
