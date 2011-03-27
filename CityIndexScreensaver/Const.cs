using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CityIndexScreensaver
{
	static class Const
	{
		public const string AppName = "City Index Screensaver";

		public static Dictionary<int, string> Markets = new Dictionary<int, string>
			{
				{ 99498,
					"Wall Street CFD"
				},
				{ 99500,
					"UK 100 CFD"
				},
				{ 99502,
					"US Tech 100 CFD"
				},
				{ 99504,
					"US SP 500 CFD"
				},
				{ 99506,
					"France 40 CFD"
				},
				{ 99508,
					"Germany 30 CFD"
				},
				{ 99510,
					"Italy 40 CFD"
				},
				{ 99524,
					"EUR/CAD (per 0.0001) CFD"
				},
				{ 99553,
					"Aldata CFD"
				},
				{ 99554,
					"Comptel CFD"
				},
				{ 99555,
					"Elcoteq Network CFD"
				},
				{ 99556,
					"F Secure CFD"
				},
				{ 99558,
					"Nokia CFD"
				},
				{ 99560,
					"Sampo Plc CFD"
				},
				{ 99561,
					"Stora Enso A CFD"
				},
				{ 99562,
					"Tieto Oyj CFD"
				},
				{ 99563,
					"UPM-Kymenne OY CFD"
				},
				{ 99564,
					"Alcoa CFD"
				},
				{ 99565,
					"General Elec Co CFD"
				},
				{ 99566,
					"Johnson and Johnson CFD"
				},
				{ 99568,
					"American Express CFD"
				},
				{ 99569,
					"Motors Liquidation CFD"
				},
				{ 99570,
					"JP Morgan Chase CFD"
				},
				{ 99571,
					"Procter and Gamble CFD"
				},
				{ 99572,
					"Boeing Co CFD"
				},
				{ 99573,
					"Home Depot CFD"
				},
				{ 99574,
					"Coca Cola Co CFD"
				},
				{ 99576,
					"Citigroup CFD"
				},
				{ 99577,
					"Honeywell International Inc CFD"
				},
				{ 99578,
					"McDonalds Corp CFD"
				},
				{ 99579,
					"ATandT CFD"
				},
				{ 99580,
					"Caterpillar CFD"
				},
				{ 99581,
					"Hewlett-Packard CFD"
				},
				{ 99582,
					"3M Company CFD"
				},
				{ 99583,
					"United Tech Cp CFD"
				},
				{ 99584,
					"Du Pont CFD"
				},
				{ 99585,
					"Intl Bus Machine CFD"
				},
				{ 99586,
					"Altria Group Inc CFD"
				},
				{ 99587,
					"Wal-Mart Stores CFD"
				},
				{ 99588,
					"Walt Disney Co CFD"
				},
				{ 99590,
					"Merck and Co CFD"
				},
				{ 99591,
					"Exxon Mobil CFD"
				},
				{ 99592,
					"Eastman Kodak CFD"
				},
				{ 99593,
					"Intl Paper Co CFD"
				},
				{ 99594,
					"Adidas AG CFD"
				},
				{ 99596,
					"Allianz SE CFD"
				},
				{ 99598,
					"BASF SE CFD"
				},
				{ 99600,
					"BMW CFD"
				},
				{ 99601,
					"Infineon Technologies AG CFD"
				},
				{ 99602,
					"Bayer CFD"
				},
				{ 99604,
					"Commerzbank CFD"
				},
				{ 99605,
					"MAN SE CFD"
				},
				{ 99606,
					"Daimler AG CFD"
				},
				{ 99608,
					"Muenchener Rueckversicherungs CFD"
				},
				{ 99609,
					"Deutsche Bank AG (EUR) CFD"
				},
				{ 99610,
					"Deutsche Post AG CFD"
				},
				{ 99612,
					"SAP CFD"
				},
				{ 99613,
					"Lufthansa AG CFD"
				},
				{ 99616,
					"Siemens (GER) CFD"
				},
				{ 99617,
					"E.ON CFD"
				},
				{ 99618,
					"ThyssenKrupp AG CFD"
				},
				{ 99620,
					"Volkswagen CFD"
				},
				{ 99621,
					"Accor CFD"
				},
				{ 99622,
					"Air Liquide CFD"
				},
				{ 99625,
					"AXA CFD"
				},
				{ 99626,
					"BNP Paribas CFD"
				},
				{ 99628,
					"CAP Gemini CFD"
				},
				{ 99629,
					"Carrefour CFD"
				},
				{ 99634,
					"LVMH Moet Hennessy CFD"
				},
				{ 99639,
					"Renault CFD"
				},
				{ 99644,
					"STMicroelectronics (FRA) CFD"
				},
				{ 99649,
					"Aberforth Smaller Companies Trust CFD"
				},
				{ 99650,
					"Vivendi Univers CFD"
				},
				{ 99651,
					"Agfa-Gevaert N.V CFD"
				},
				{ 99654,
					"Bekaert CFD"
				},
				{ 99655,
					"Colruyt CFD"
				},
				{ 99657,
					"Delhaize Freres Ps CFD"
				},
				{ 99663,
					"Groupe Bruxelles Lambert CFD"
				},
				{ 99664,
					"KBC Groep NV CFD"
				},
				{ 99665,
					"Solvay CFD"
				},
				{ 99668,
					"UCB CFD"
				},
				{ 99669,
					"Umicore CFD"
				},
				{ 99671,
					"AEGON CFD"
				},
				{ 99672,
					"AHOLD CFD"
				},
				{ 99677,
					"Reed Elsevier NV CFD"
				},
				{ 99680,
					"Heineken CFD"
				},
				{ 99684,
					"Koninklijke Philips Electronics CFD"
				},
				{ 99691,
					"Anglo American CFD"
				},
				{ 99692,
					"B Sky B (LSE) CFD"
				},
				{ 99693,
					"Marks and Spencer CFD"
				},
				{ 99695,
					"Associated British Foods CFD"
				},
				{ 99696,
					"BT Group CFD"
				},
				{ 99697,
					"GKN Plc CFD"
				},
				{ 99698,
					"Schroders CFD"
				},
				{ 99701,
					"Glaxosmithkline CFD"
				},
				{ 99703,
					"Hays plc CFD"
				},
				{ 99704,
					"ARM Holdings CFD"
				},
				{ 99706,
					"Old Mutual (LSE) CFD"
				},
				{ 99707,
					"Sage Group CFD"
				},
				{ 99709,
					"Prudential CFD"
				}
			};
	}
}
