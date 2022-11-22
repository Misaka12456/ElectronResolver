using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MisakaCastle.ElectronResolver.Core
{
	public class ElectronAppSource
	{
		public string AppName { get; private set; } = string.Empty;

		public string AppNameGuessed { get; private set; } = string.Empty;

		public string AppLauncherPath { get; private set; } = string.Empty;

		public ElectronAppUninstaller? Uninstaller { get; set; } = null;

		public ImageSource AppIconSource
		{
			get
			{
				return ShellIcon.ToImageSource(ShellIcon.GetLargeIcon(AppLauncherPath));
			}
		}

		public static ElectronAppSource FromEAppInfo(ElectronAppInfo app)
		{
			return new ElectronAppSource()
			{
				AppName = app.Name,
				AppNameGuessed = !string.IsNullOrEmpty(app.NameGuessed) ? app.NameGuessed : app.Name,
				AppLauncherPath = app.InstallPath
			};
		}
	}
}
