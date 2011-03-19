using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace CityIndexScreensaver
{
	public class ApplicationSettings
	{
		public ApplicationSettings()
		{
			var prices = new[] { "99500", "99502", "99504", "99506", };
			PricesToWatchString = string.Join(",", prices);
		}

		public string ServerUrl { get; set; }
		public string PricesToWatchString { get; set; }
		public string[] PricesToWatch
		{
			get
			{
				var res = PricesToWatchString.Split(',');
				for (int i = 0; i < res.Length; i++)
					res[i] = "PRICES.PRICE." + res[i];
				return res;
			}
		}

		private static ApplicationSettings _instance;

		public static ApplicationSettings Instance
		{
			get { return _instance ?? (_instance = Load()); }
		}

		private const string FileName = @"ApplicationSettings.xml";

		public static void Reload()
		{
			_instance = Load();
		}

		public static ApplicationSettings Load()
		{
			var settings = new ApplicationSettings();

			using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
			{
				if (!store.FileExists(FileName))
					return settings;

				using (var isoStream = store.OpenFile(FileName, FileMode.Open))
				{
					var s = new XmlSerializer(typeof(ApplicationSettings));
					using (var rd = new StreamReader(isoStream))
					{
						settings = (ApplicationSettings)s.Deserialize(rd);
					}

					return settings;
				}
			}
		}

		public void Save()
		{
			using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
			{
				using (var isoStream = store.OpenFile(FileName, FileMode.Create))
				{
					var s = new XmlSerializer(typeof(ApplicationSettings));
					using (var writer = new StreamWriter(isoStream))
					{
						s.Serialize(writer, this);
					}
				}
			}
		}
	}
}
