#pragma warning disable CA1416
using System.Security.Principal;

namespace System.Enhance
{

	public static class PermissionHelper
	{
		public static bool IsRunAsAdmin()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
					var current = WindowsIdentity.GetCurrent();
					var principal = new WindowsPrincipal(current);
					return principal.IsInRole(WindowsBuiltInRole.Administrator);
				case PlatformID.Unix: // TODO: Find a way to check whether current user is root(user id=0)
					return true;
				default:
					return false;
			}
		}
	}
}
