using System.Windows;

namespace MisakaCastle.ElectronResolver
{
	public class Program
	{
		public static int Main(string[] args)
		{
			switch (args.Length)
			{
				case 0:
					var app = new Application();
					return app.Run(new Win_Main());
				default:
					return 1;
			}
		}
	}
}
