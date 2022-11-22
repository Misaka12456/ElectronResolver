using System;
using System.Diagnostics;
using System.IO;

namespace MisakaCastle.ElectronResolver.Core
{
	public class ElectronAppInfo
	{
		public string Name { get; } = string.Empty;

		public string? NameGuessed { get; } = null;

		public string Company { get; } = string.Empty;

		private string versionStr = string.Empty;

		public Version? Version { get => !string.IsNullOrEmpty(versionStr) ? Version.Parse(versionStr.Split('-')[0].Split('_')[0]) : null; }

		public string InstallPath { get; } = string.Empty;

		public ElectronAppInfo(string name, string company, string versionStr, string installPath)
		{
			Name = name;
			NameGuessed = new FileInfo(installPath).Directory?.Name;
			Company = company;
			this.versionStr = versionStr;
			InstallPath = installPath;
		}

		public ElectronAppInfo(string name, string company, string versionStr, FileInfo installInfo)
		{
			Name = name;
			NameGuessed = installInfo.Directory?.Name;
			Company = company;
			this.versionStr = versionStr;
			InstallPath = installInfo.FullName;
		}

		public ElectronAppInfo(FileVersionInfo launcherInfo, string installPath)
		{
			Name = launcherInfo.ProductName ?? string.Empty;
			NameGuessed = new FileInfo(installPath).Directory?.Name;
			Company = launcherInfo.CompanyName ?? string.Empty;
			versionStr = launcherInfo.ProductVersion ?? string.Empty;
			InstallPath = installPath;
		}
	}
}
