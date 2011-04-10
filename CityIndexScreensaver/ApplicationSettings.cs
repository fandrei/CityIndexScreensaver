using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security;
using System.Xml.Serialization;

namespace CityIndexScreensaver
{
	public class ApplicationSettings
	{
		public ApplicationSettings()
		{
			ServerUrl = "https://ciapipreprod.cityindextest9.co.uk/tradingapi";
			StreamingServerUrl = "https://pushpreprod.cityindextest9.co.uk/CITYINDEXSTREAMING";
			UserName = "xx189949";
			Password = "password";

			var prices = new[] { 99500, 99502, 99504, 99506, };
			PricesToWatchString = IdsToString(prices);

			NewsMaxCount = 30;
			NewsCategory = "NEWS.MOCKHEADLINES.UK";

			GraphPeriodSecs = 300;
		}

		public string ServerUrl { get; set; }
		public string StreamingServerUrl { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public int NewsMaxCount { get; set; }
		public string NewsCategory { get; set; }

		public int GraphPeriodSecs { get; set; }

		public string PricesToWatchString { get; set; }
		public int[] PricesToWatch
		{
			get { return StringToIds(PricesToWatchString); }
			set { PricesToWatchString = IdsToString(value); }
		}

		static int[] StringToIds(string ids)
		{
			var tmp = ids.Split(new[] { PricesDelimiter }, StringSplitOptions.RemoveEmptyEntries);
			var res = (from id in tmp select int.Parse(id)).ToArray();
			return res;
		}

		static string IdsToString(int[] ids)
		{
			var res = string.Join(PricesDelimiter, ids);
			return res;
		}

		const string PricesDelimiter = " ";

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
