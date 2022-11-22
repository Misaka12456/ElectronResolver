using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MisakaCastle.ElectronResolver.Core
{
	public class ElectronAppUninstaller
	{
		public static event EventHandler<string>? OnAppUninstRegistryScanned;

		public ElectronAppSource AppSource { get; private set; }

		private static Dictionary<string, RegistryKey> uninstallKeys = new();

		public ElectronAppUninstaller(ElectronAppSource appSource)
		{
			AppSource = appSource;
		}

		private bool TryFindRegistryKey(out RegistryKey? uninstKey)
		{
			try
			{
				LoadUninstallKeys();
				if (uninstallKeys.TryGetValue(AppSource.AppName, out uninstKey) || uninstallKeys.TryGetValue(AppSource.AppNameGuessed, out uninstKey))
				{
					return true;
				}
				else
				{
					var r = uninstallKeys.Where(pair => (string?)pair.Value.GetValue("DisplayName") == AppSource.AppName ||
							(string?)pair.Value.GetValue("DisplayName") == AppSource.AppNameGuessed);
					if (r.Any())
					{
						uninstKey = r.First().Value;
						return true;
					}
					else
					{
						r = uninstallKeys.Where(pair => ((string?)pair.Value.GetValue("DisplayIcon"))?.Split(',')[0] == AppSource.AppLauncherPath);
						if (r.Any())
						{
							uninstKey = r.First().Value;
							return true;
						}
						else
						{
							uninstKey = null;
							return false;
						}
					}
				}
			}
			catch (SecurityException)
			{
				uninstKey = null;
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				uninstKey = null;
				return false;
			}
		}

		public static void LoadUninstallKeys()
		{
			if (!uninstallKeys.Any())
			{
				var hklm = Registry.LocalMachine;
				using var baseKey = hklm.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")!;
				foreach (string subKeyName in baseKey.GetSubKeyNames())
				{
					var subKey = baseKey.OpenSubKey(subKeyName);
					if (subKey != null)
					{
						OnAppUninstRegistryScanned?.Invoke(null, subKey.ToString());
						uninstallKeys.Add(subKeyName, subKey);
					}
				}
			}
		}

		/// <summary>
		/// Uninstall current Electron application.
		/// </summary>
		/// <exception cref="AppUninstallException" />
		public void UninstallMain()
		{
			if (TryFindRegistryKey(out var uninstKey))
			{
				string? uninstProgram = (string?)uninstKey!.GetValue("UninstallString");
				if (uninstKey.GetValue("QuietUninstallString") != null)
				{
					uninstProgram = (string)uninstKey!.GetValue("QuietUninstallString")!;
				}
				if (!string.IsNullOrEmpty(uninstProgram))
				{
					if (MessageBox.Show(I.S["Uninstall.Confirm"], "Electron Resolver", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					{
						string uninstPath = uninstProgram.Split(new[] { '/' })[0];
						string arguments = uninstProgram.Split(new[] { '/' })[1];
						Process.Start(new ProcessStartInfo()
						{
							FileName = uninstPath,
							Arguments = arguments,
							WindowStyle = ProcessWindowStyle.Normal
						});
					}
				}
				else
				{
					throw new AppUninstallException(I.S["Uninstall.RegKeyNotFound"], new KeyNotFoundException());
				}
			}
			else
			{
				throw new AppUninstallException(I.S["Uninstall.RegKeyNotFound"], new KeyNotFoundException());
			}
		}

		private static bool disposedValue = false;

		public static void StaticDispose()
		{
			if (!disposedValue)
			{
				disposedValue = true;
				if (uninstallKeys.Any())
				{
					uninstallKeys.Values.ToList().ForEach(x => x.Dispose());
					uninstallKeys.Clear();
				}
			}
		}
	}

	public class AppUninstallException : Exception
	{
		public AppUninstallException() : base()
		{
		}

		public AppUninstallException(string? message) : base(message)
		{
		}

		public AppUninstallException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
