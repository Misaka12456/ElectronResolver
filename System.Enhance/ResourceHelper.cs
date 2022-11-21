using System.Reflection;

namespace System.Enhance
{
	public static class ResourceHelper
	{
		public static byte[] ReadResource(this Assembly assembly, string fileName)
		{
			var stream = assembly.GetManifestResourceStream(fileName);
			if (stream != null)
			{
				try
				{
					byte[] buffer = new byte[stream.Length];
					stream.Read(buffer);
					return buffer;
				}
				finally
				{
					stream.Close();
				}
			}
			else
			{
				return Array.Empty<byte>();
			}
		}
	}
}