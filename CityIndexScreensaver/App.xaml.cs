using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
			var args = e.Args;

			if (args.Length == 0)
			{
				Application.Current.Shutdown();
				return;
			}

			var firstArg = args[0].ToLower();

			// config
			if (firstArg == "/c")
			{
				Application.Current.Shutdown();
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
				Application.Current.Shutdown();
				return;
			}

			Application.Current.Shutdown();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{

		}
	}
}
