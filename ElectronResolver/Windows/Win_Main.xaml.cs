#pragma warning disable 8618
using MisakaCastle.ElectronResolver.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Documents;

namespace MisakaCastle.ElectronResolver
{
	/// <summary>
	/// Interaction logic for Win_Main.xaml
	/// </summary>
	public partial class Win_Main : Window
	{
		public static Win_Main Instance { get; private set; }

		public Win_Main()
		{
			InitializeComponent();
			Instance = this;
			Title += $" - Alpha {Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)} (Development Build)";
			lbl_Status.Content = I.S["General.Ready"];
		}

		internal void RefreshList(List<ElectronAppInfo> infoList, bool isInterrupted  = false)
		{
			if (!isInterrupted)
			{
				var appSources = ElectronAppHelper.ElectronApps.Select(a => ElectronAppSource.FromEAppInfo(a)).ToList();
				for (int i = 0; i < appSources.Count; i++)
				{
					var uninstaller = new ElectronAppUninstaller(appSources[i]);
					appSources[i].Uninstaller = uninstaller;
				}
				lbx_eApps.ItemsSource = appSources;
				lbl_Status.Content = $"Found {infoList.Count} Electron-based applications.";
			}
			else
			{
				lbl_Status.Content = $"Search procedure was interrupted. Found {infoList.Count} Electron-based applications.";
			}
		}

		private void Uninstaller_OnAppUninstRegistryScanned(object? sender, string e)
		{
			Dispatcher.Invoke(() =>
			{
				lbl_Status.Content = string.Format(I.S["Searching.UninstReg"], e);
				Debug.WriteLine(string.Format(I.S["Searching.UninstReg"], e));
			});
		}

		private void Tsmi_StartSearch_Click(object sender, RoutedEventArgs e)
		{
			new Thread(async () =>
			{
				Dispatcher.Invoke(() =>
				{
					lbl_Status.Content = string.Format(I.S["Searching.Information"], "Loading");
					tsmi_StartSearch.IsEnabled = false;
				});
				ElectronAppHelper.OnSearchFolderChanged += EAppHelper_OnSearchFolderChanged;
				var r = await ElectronAppHelper.SearchElectronAppsAsync();
				ElectronAppHelper.ElectronApps = r;
				ElectronAppHelper.SaveEAppList();
				ElectronAppUninstaller.OnAppUninstRegistryScanned += Uninstaller_OnAppUninstRegistryScanned;
				ElectronAppUninstaller.LoadUninstallKeys();
				Dispatcher.Invoke(() =>
				{
					RefreshList(ElectronAppHelper.ElectronApps, ElectronAppHelper.LastResultType == SearchResultType.Interrupted);
					lbx_eApps.IsEnabled = true;
					tsmi_StartSearch.IsEnabled = true;
				});
			})
			{ IsBackground = true }.Start();
		}
		private void EAppHelper_OnSearchFolderChanged(string currentPath)
		{
			Dispatcher.Invoke(() =>
			{
				if (currentPath == "Completed")
				{
					ElectronAppHelper.OnSearchFolderChanged -= EAppHelper_OnSearchFolderChanged;
				}
				else
				{
					lbl_Status.Content = string.Format(I.S["Searching.Information"], currentPath);
				}
			});
		}
	}
}
