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
	public class ElectronSearcher
	{
		public List<ElectronAppInfo> ElectronApps { get; private set; } = new List<ElectronAppInfo>();
		private Task searchTask;
		private CancellationTokenSource searchCts;
		private SearchResultType lastResultType = SearchResultType.Unknown;

		public event EventHandler<string>? OnSearchFolderChanged;

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
						if (dir.Name == "Windows")
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
				using var appAsarStream = new MemoryStream();
				try
				{
					Frm_Search.Instance.Invoke(() => OnSearchFolderChanged?.Invoke(this, dir.FullName));
					bool isAppAsarExists = File.Exists(Path.Combine(dir.FullName, "resources", "app.asar"));
					string appNameGuessed = dir.Name;
					if (!isAppAsarExists)
					{
						if (File.Exists(Path.Combine(dir.FullName, appNameGuessed + ".exe")))
						{
							var zipLauncher = ZipFile.Read(Path.Combine(dir.FullName, appNameGuessed + ".exe"));
							if (zipLauncher != null)
							{
								var innerAsar = zipLauncher["resources/app.asar"];
								if (innerAsar != null)
								{
									using var innerAsarReader = innerAsar.OpenReader();
									byte[] innerAsarData = new byte[innerAsar.UncompressedSize];
									innerAsarReader.Read(innerAsarData, 0, innerAsarData.Length);
									innerAsarReader.Close();
									appAsarStream.Write(innerAsarData, 0, innerAsarData.Length);
								}
							}
						}
					}
					else
					{
						appAsarStream.Write(File.ReadAllBytes(Path.Combine(dir.FullName, "resources", "app.asar")));
					}
					if (appAsarStream.Length > 0)
					{
						appAsarStream.Seek(0, SeekOrigin.Begin);
						using var reader = new AsarReader(appAsarStream);
						var appBaseInfo = reader.ReadAppBaseInfo();
						reader.Close();
						ElectronApps.Add(appBaseInfo.ToAppInfo(appNameGuessed, dir.FullName));
					}
				}
				catch
				{

				}
				finally
				{
					dirs.Dequeue();
					appAsarStream.Dispose();
				}
			}
			Frm_Search.Instance.Invoke(() => OnSearchFolderChanged?.Invoke(this, "Completed"));
		}

		public void Abort()
		{
			searchCts.Cancel();
		}
	}
}
