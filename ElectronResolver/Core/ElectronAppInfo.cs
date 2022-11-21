using System;
using System.Diagnostics;
using System.IO;

namespace MisakaCastle.ElectronResolver.Core
{
	public class ElectronAppInfo
	{
		public string Name { get; } = string.Empty;

		public string Company { get; } = string.Empty;

		private string versionStr = string.Empty;

		public Version? Version { get => !string.IsNullOrEmpty(versionStr) ? Version.Parse(versionStr) : null; }

		public string InstallPath { get; } = string.Empty;

		public ElectronAppInfo(string name, string company, string versionStr, string installPath)
		{
			Name = name;
			Company = company;
			this.versionStr = versionStr;
			InstallPath = installPath;
		}

		public ElectronAppInfo(string name, string company, string versionStr, FileInfo installInfo)
		{
			Name = name;
			Company = company;
			this.versionStr = versionStr;
			InstallPath = installInfo.FullName;
		}

		public ElectronAppInfo(FileVersionInfo launcherInfo, string installPath)
		{
			Name = launcherInfo.ProductName ?? string.Empty;
			Company = launcherInfo.CompanyName ?? string.Empty;
			versionStr = launcherInfo.ProductVersion ?? string.Empty;
			InstallPath = installPath;
		}
	}
}
