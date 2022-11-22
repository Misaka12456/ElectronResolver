using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace MisakaCastle.ElectronResolver.Core
{
	public interface IElectronAppInfoBase
	{
		public string Name { get; set; }

		public string Version { get; set; }

		public ElectronAppAuthor Author { get; set; }

		public string MainScript { get; set; }

		public ElectronAppInfo ToAppInfo(string appNameGuessed, string installFolderPath);
	}

	public struct ElectronAppInfoBase : IElectronAppInfoBase
	{
		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("version")]
		public string Version { get; set; } = string.Empty;

		[JsonProperty("author")]
		public ElectronAppAuthor Author { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; } = string.Empty;

		[JsonProperty("license", NullValueHandling = NullValueHandling.Include)]
		public string? License { get; set; } = null;

		[JsonProperty("main")]
		public string MainScript { get; set; } = string.Empty;

		[JsonProperty("electronLanguagesInfoPlistStrings")]
		public Dictionary<string, ElectronLangPlistString> InfoPlistStrings { get; set; } = new();

		[JsonProperty("dependencies")]
		public Dictionary<string, string> Dependencies { get; set; } = new();

		public ElectronAppInfo ToAppInfo(string appNameGuessed, string installFolderPath)
		{
			string launcherPath = Path.Combine(installFolderPath, appNameGuessed + ".exe");
			foreach (var finder in ElectronAppHelper.LauncherFinders)
			{
				if (finder.TryFindElectronLauncher(this, installFolderPath, out launcherPath!))
				{
					break;
				}
			}
			return new ElectronAppInfo(Name, Author.Name ?? string.Empty, Version, launcherPath);
		}
	}

	public struct ElectronAppInfoExtractedBase : IElectronAppInfoBase
	{
		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("version")]
		public string Version { get; set; } = string.Empty;

		[JsonProperty("distro")]
		public string? Distro { get; set; } = null;

		[JsonProperty("author")]
		public ElectronAppAuthor Author { get; set; }

		[JsonProperty("license", NullValueHandling = NullValueHandling.Include, Required = Required.AllowNull)]
		public string? License { get; set; } = null;

		[JsonProperty("main")]
		public string MainScript { get; set; } = string.Empty;

		[JsonProperty("private")]
		public bool IsPrivatePackage { get; set; } = false;

		[JsonProperty("scripts")]
		public Dictionary<string, string> Scripts { get; set; } = new();

		[JsonProperty("dependencies")]
		public Dictionary<string, string> Dependencies { get; set; } = new();

		[JsonProperty("devDependencies")]
		public Dictionary<string, string> DevDependencies { get; set; } = new();

		[JsonProperty("optionalDependencies")]
		public Dictionary<string, string> OptionalDependencies { get; set; } = new();

		[JsonProperty("repository")]
		public ElectronUrlInfo? Repository { get; set; } = null;

		[JsonProperty("bugs")]
		public ElectronUrlInfo? BugsReport { get; set; } = null;

		[JsonProperty("resolutions")]
		public Dictionary<string, string> Resolutions { get; set; } = new();

		public ElectronAppInfo ToAppInfo(string appNameGuessed, string installFolderPath)
		{
			string launcherPath = Path.Combine(installFolderPath, appNameGuessed + ".exe");
			foreach (var finder in ElectronAppHelper.LauncherFinders)
			{
				if (finder.TryFindElectronLauncher(this, installFolderPath, out launcherPath!))
				{
					break;
				}
			}
			return new ElectronAppInfo(Name, Author.Name ?? string.Empty, Version, launcherPath);
		}
	}

	public struct ElectronAppAuthor
	{
		[JsonProperty("name")]
		public string? Name { get; set; }

		[JsonProperty("email", Required = Required.DisallowNull)]
		public string? Email { get; set; } = null;

		public static implicit operator string(ElectronAppAuthor author)
		{
			if (author == null)
			{
				return string.Empty;
			}
			return author.Name ?? string.Empty;
		}

		public static implicit operator ElectronAppAuthor(string authorStr)
		{
			return new()
			{
				Name = authorStr,
				Email = null
			};
		}
	}

	public struct ElectronLangPlistString
	{
		[JsonProperty("CFBundleDisplayName")]
		public string DisplayName { get; set; }

		[JsonProperty("CFBundleName")]
		public string Name { get; set; }
	}

	public struct ElectronUrlInfo
	{
		[JsonProperty("type")]
		public string Type { get; set; } = string.Empty;

		[JsonProperty("url")]
		public string Url { get; set; } = string.Empty;
	}
}
