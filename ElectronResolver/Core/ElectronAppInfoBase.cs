using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace MisakaCastle.ElectronResolver.Core
{
	public struct ElectronAppInfoBase
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("author")]
		public ElectronAppAuthor Author { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("license", NullValueHandling = NullValueHandling.Include, Required = Required.AllowNull)]
		public string? License { get; set; } 

		[JsonProperty("main")]
		public string MainScript { get; set; }

		[JsonProperty("electronLanguagesInfoPlistStrings")]
		public Dictionary<string, ElectronLangPlistString> InfoPlistStrings { get; set; }

		[JsonProperty("dependencies")]
		public Dictionary<string, string> Dependencies { get; set; }

		public ElectronAppInfo ToAppInfo(string appNameGuessed, string installFolderPath)
		{
			return new ElectronAppInfo(Name, Author.Name ?? string.Empty, Version, Path.Combine(installFolderPath, appNameGuessed + ".exe"));
		}
	}

	public struct ElectronAppAuthor
	{
		[JsonProperty("name")]
		public string? Name { get; set; }

		[JsonProperty("email")]
		public string? Email { get; set; }
	}

	public struct ElectronLangPlistString
	{
		[JsonProperty("CFBundleDisplayName")]
		public string DisplayName { get; set; }

		[JsonProperty("CFBundleName")]
		public string Name { get; set; }
	}
}
