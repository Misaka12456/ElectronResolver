using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Enhance;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MisakaCastle.ElectronResolver.Core
{
	public delegate void ElectronSearchHandler(object? sender, string currentPath, SearchResultType? resultType = null);

	public class ElectronSearcher
	{
		public List<ElectronAppInfo> ElectronApps { get; private set; } = new List<ElectronAppInfo>();
		private Task searchTask;
		private CancellationTokenSource searchCts;
		private SearchResultType lastResultType = SearchResultType.Unknown;

		public event ElectronSearchHandler? OnSearchFolderChanged;

		public ElectronSearcher(out bool isLimitedPermission)
		{
			isLimitedPermission = !PermissionHelper.IsRunAsAdmin();
			searchTask = new Task(SearchMain);
			searchCts = new CancellationTokenSource();
		}

		public void StartSearch()
		{
			searchTask.Start();
		}

		private void SearchMain()
		{
			ElectronApps.Clear();
			string[] drives = Directory.GetLogicalDrives();
			var dirs = new Queue<DirectoryInfo>();
			foreach (string drive in drives)
			{
				if (searchCts.IsCancellationRequested)
				{
					lastResultType = SearchResultType.Interrupted;
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
						Frm_Search.Instance.Invoke(() => OnSearchFolderChanged?.Invoke(this, "PreSearching:" + dir.FullName));
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
					lastResultType = SearchResultType.Interrupted;
					break;
				}
				var dir = dirs.First();
				try
				{
					Frm_Search.Instance.Invoke(() => OnSearchFolderChanged?.Invoke(this, dir.FullName));
					foreach (var finder in IElectronFinder.Finders)
					{
						if (finder.TryFindElectronApp(dir.FullName, out var app))
						{
							ElectronApps.Add(app!);
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
			Frm_Search.Instance.Invoke(() => OnSearchFolderChanged?.Invoke(this, "Completed", lastResultType));
		}

		public void Abort()
		{
			searchCts.Cancel();
		}
	}
}
