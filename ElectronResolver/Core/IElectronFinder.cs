using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MisakaCastle.ElectronResolver.Core
{
	public interface IElectronFinder
	{
		public bool TryFindElectronApp(string path, out ElectronAppInfo? info);

		public static List<IElectronFinder> Finders = new List<IElectronFinder>()
		{
			new ElectronPackedFinder(),
			new ElectronExtractedFinder()
		};
	}

	public class ElectronPackedFinder : IElectronFinder
	{
		public bool TryFindElectronApp(string path, out ElectronAppInfo? info)
		{
			using var appAsarStream = new MemoryStream();
			try
			{
				var dir = new DirectoryInfo(path);
				string appNameGuessed = dir.Name;
				if (!File.Exists(Path.Combine(path, "resources", "app.asar")))
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
					info = appBaseInfo.ToAppInfo(appNameGuessed, path);
					return true;
				}
				else
				{
					info = null;
					return false;
				}
			}
			catch (ZipException)
			{
				info = null;
				return false;
			}
			catch (SecurityException)
			{
				info = null;
				return false;
			}
			catch (IOException)
			{
				info = null;
				return false;
			}
			finally
			{
				appAsarStream.Close();
			}
		}
	}

	public class ElectronExtractedFinder : IElectronFinder
	{
		public bool TryFindElectronApp(string path, out ElectronAppInfo? info)
		{
			try
			{
				var dir = new DirectoryInfo(path);
				string appNameGuessed = dir.Name;
				if (File.Exists(Path.Combine(path, "resources", "app", "package.json")))
				{
					var appBaseInfo = JsonConvert.DeserializeObject<ElectronAppInfoExtractedBase>(File.ReadAllText(
						Path.Combine(path, "resources", "app", "package.json"), Encoding.UTF8));
					info = appBaseInfo.ToAppInfo(appNameGuessed, path);
					return true;
				}
				else
				{
					info = null;
					return false;
				}
			}
			catch (SecurityException)
			{
				info = null;
				return false;
			}
			catch (IOException)
			{
				info = null;
				return false;
			}
		}
	}
}
