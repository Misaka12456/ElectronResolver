using Paraparty.JsonChan;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Enhance;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MisakaCastle.ElectronResolver.Core
{
	public class I
	{
		public static I S { get; private set; }

		public static string CurrentLangCode { get => CultureInfo.CurrentCulture.Name; }

		public IDictionary<string, string> CurrentLangList { get => FullLangList.TryGetValue(CurrentLangCode, out var currLangList) ? currLangList : new Dictionary<string, string>(); }

		public Dictionary<string, IDictionary<string, string>> FullLangList { get; }

		public string this[string key] { get => CurrentLangList.TryGetValue(key, out string? value) ? value : throw new KeyNotFoundException($"Key '{key}' not found"); }

		static I()
		{
			var dict = new Dictionary<string, IDictionary<string, string>>();
			try
			{
				var asm = Assembly.GetExecutingAssembly();
				asm.GetManifestResourceNames().Where(name => name.StartsWith("MisakaCastle.ElectronResolver.Languages") && name.EndsWith(".json")).ToList()
					.ForEach(fileName =>
					{
						string langCode = fileName[(fileName.Replace(".json", string.Empty).LastIndexOf('.') + 1)..]; // Substring
					byte[] data = asm.ReadResource(fileName);
						if (data.Any())
						{
							string str = Encoding.UTF8.GetString(data);
							var subDict = Json.Parse(str) as IDictionary<string, string>;
							if (subDict != null)
							{
								dict.Add(langCode, subDict);
							}
						}
					});
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("An unexpected error occurred when loading internationalization(i18n) language data: {0}", ex);
			}
			S = new I(dict);
		}

		private I(Dictionary<string, IDictionary<string, string>> i18nList)
		{
			FullLangList = i18nList;
		}
	}
}
