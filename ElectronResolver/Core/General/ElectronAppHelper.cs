using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MisakaCastle.ElectronResolver.Core
{
	public delegate void ElectronSearchHandler(string currentPath);
	public class ElectronAppHelper
	{
		public static List<ElectronAppInfo> ElectronApps { get; set; }
		public static List<IElectronFinder> Finders => new List<IElectronFinder>()
		{
			new ElectronPackedFinder(),
			new ElectronExtractedFinder()
		};

		public static List<IElectronLauncherFinder> LauncherFinders => new List<IElectronLauncherFinder>()
		{
			new NameLauncherFinder(),
			new AuthorLauncherFinder()
		};

		private static CancellationTokenSource searchCts;
		public static SearchResultType LastResultType { get; private set; } = SearchResultType.Unknown;

		public static event ElectronSearchHandler? OnSearchFolderChanged;

		static ElectronAppHelper()
		{
			ElectronApps = new List<ElectronAppInfo>();
			searchCts = new CancellationTokenSource();
			LastResultType = SearchResultType.Unknown;
		}

		public static async Task<List<ElectronAppInfo>> SearchElectronAppsAsync()
		{
			return await Task.Run(() =>
			{
				var apps = new List<ElectronAppInfo>();
				string[] drives = Directory.GetLogicalDrives();
				var dirs = new Queue<DirectoryInfo>();
				foreach (string drive in drives)
				{
					if (searchCts.IsCancellationRequested)
					{
						LastResultType = SearchResultType.Interrupted;
						break;
					}
					try
					{
						foreach (var dir in new DirectoryInfo(drive).EnumerateDirectories("*", new EnumerationOptions() { RecurseSubdirectories = true, MaxRecursionDepth = 256 }))
						{
							if (dir.Name == "Windows" && dir.Root.FullName == drive) // Ignore \Windows folder in the root directory of every drive
							{
								break;
							}
							Win_Main.Instance.Dispatcher.Invoke(() => OnSearchFolderChanged?.Invoke("PreSearching:" + dir.FullName));
							if (searchCts.IsCancellationRequested)
							{
								break;
							}
							dirs.Enqueue(dir);
						}
					}
					catch { }
				}
				while (dirs.Count > 0)
				{
					if (searchCts.IsCancellationRequested)
					{
						LastResultType = SearchResultType.Interrupted;
						break;
					}
					var dir = dirs.First();
					try
					{
						Win_Main.Instance.Dispatcher.Invoke(() => OnSearchFolderChanged?.Invoke(dir.FullName));
						foreach (var finder in Finders)
						{
							if (finder.TryFindElectronApp(dir.FullName, out var app))
							{
								apps.Add(app!);
								break;
							}
						}
					}
					catch
					{

					}
					finally
					{
						dirs.Dequeue();
					}
				}
				Win_Main.Instance.Dispatcher.Invoke(() => OnSearchFolderChanged?.Invoke("Completed"));
				return apps;
			});
		}

		public static void SaveEAppList()
		{
			var r = JArray.FromObject(ElectronApps);
			File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "config.dat"), Convert.ToBase64String(Encoding.UTF8.GetBytes(r.ToString())), Encoding.ASCII);
		}

		public static void LoadEAppList()
		{
			string r = Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "config.dat"), Encoding.ASCII)));
			ElectronApps = JArray.Parse(r).ToObject<List<ElectronAppInfo>>()!;
		}
	}
}
