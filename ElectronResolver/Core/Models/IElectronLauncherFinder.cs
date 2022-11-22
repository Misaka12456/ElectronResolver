using System.IO;

namespace MisakaCastle.ElectronResolver.Core
{
	public interface IElectronLauncherFinder
	{
		public bool TryFindElectronLauncher(IElectronAppInfoBase info, string path, out string? launcherPath);
	}

	public class NameLauncherFinder : IElectronLauncherFinder
	{
		public bool TryFindElectronLauncher(IElectronAppInfoBase info, string path, out string? launcherPath)
		{
			string? appNameGuessed = new DirectoryInfo(path).Name;
			if (File.Exists(Path.Combine(path, info.Name + ".exe")))
			{
				launcherPath = Path.Combine(path, info.Name + ".exe");
				return true;
			}
			else if (!string.IsNullOrEmpty(appNameGuessed) && File.Exists(Path.Combine(path, appNameGuessed + ".exe")))
			{
				launcherPath = Path.Combine(path, appNameGuessed + ".exe");
				return true;
			}
			else
			{
				launcherPath = null;
				return false;
			}
		}
	}

	public class AuthorLauncherFinder : IElectronLauncherFinder
	{
		public bool TryFindElectronLauncher(IElectronAppInfoBase info, string path, out string? launcherPath)
		{
			if (File.Exists(Path.Combine(path, info.Author.Name + ".exe")))
			{
				launcherPath = Path.Combine(path, info.Author.Name + ".exe");
				return true;
			}
			else
			{
				launcherPath = null;
				return false;
			}
		}
	}
}
