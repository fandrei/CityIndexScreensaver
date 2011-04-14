using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

using CIAPI.DTO;

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

			NewsMaxCount = 30;
			NewsCategory = "NEWS.MOCKHEADLINES.UK";

			GraphPeriodSecs = 300;

			PricesToWatch = new MarketDTO[0];
		}

		public string ServerUrl { get; set; }
		public string StreamingServerUrl { get; set; }
		public string UserName { get; set; }

		public string PasswordEncrypted { get; set; }
		static readonly byte[] AdditionalEntropy = { 0x55, 0x7F, 0xFA, 0x1A, 0x09 };

		[XmlIgnore]
		public string Password
		{
			get
			{
				var encrypted = Convert.FromBase64String(PasswordEncrypted);
				var data = ProtectedData.Unprotect(encrypted, AdditionalEntropy, DataProtectionScope.CurrentUser);
				var res = Encoding.UTF8.GetString(data);
				return res;
			}
			set
			{
				var data = Encoding.UTF8.GetBytes(value);
				var encrypted = ProtectedData.Protect(data, AdditionalEntropy, DataProtectionScope.CurrentUser);
				PasswordEncrypted = Convert.ToBase64String(encrypted);
			}
		}

		public int NewsMaxCount { get; set; }
		public string NewsCategory { get; set; }

		public int GraphPeriodSecs { get; set; }

		public MarketDTO[] PricesToWatch { get; set; }

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
