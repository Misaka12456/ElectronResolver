using System;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Numerics;
using System.Diagnostics;
using System.Linq;

namespace MisakaCastle.ElectronResolver.Core
{
	public class AsarReader : IDisposable
	{
		private BinaryReader _reader;

		public AsarReader(Stream stream)
		{
			_reader = new BinaryReader(stream);
		}

		public ElectronAppInfoBase ReadAppBaseInfo()
		{
			try
			{
				_reader.BaseStream.Seek(12, SeekOrigin.Begin); // 12-15: Asar File List Json size
				int fileListSize = _reader.ReadInt32();
				byte[] fileRawList = _reader.ReadBytes(fileListSize);
				var fileList = JObject.Parse(Encoding.UTF8.GetString(fileRawList));
				var packageJsonInfo = fileList.Value<JObject>("files")!.Value<JObject>("package.json")!;
				int packageJsonSize = packageJsonInfo.Value<int>("size");
				var packageJsonOffset = BigInteger.Parse(packageJsonInfo.Value<string>("offset")!);
				if (packageJsonOffset > 0)
				{
					_reader.BaseStream.Seek((long)packageJsonOffset, SeekOrigin.Current);
				}
				var packageJsonRaw = _reader.ReadBytes(packageJsonSize).ToList();
				if (packageJsonRaw.ToList().FindIndex(b => b == '{') != 0) // Full Data Coordinate
				{
					int idx = packageJsonRaw.ToList().FindIndex(b => b == '{');
					_reader.ReadBytes(idx).ToList().ForEach(b => packageJsonRaw.Add(b));
				}
				Debug.WriteLine(Encoding.UTF8.GetString(packageJsonRaw.ToArray()));
				var appInfo = JsonConvert.DeserializeObject<ElectronAppInfoBase>(Encoding.UTF8.GetString(packageJsonRaw.ToArray()))!;
				return appInfo;
			}
			catch(Exception ex)
			{
				throw new IOException("Invalid asar data", ex);
			}
			finally
			{
				_reader.BaseStream.Seek(0, SeekOrigin.Begin);
			}
		}

		public void Close()
		{
			_reader.Close();
		}

		private bool disposedValue = false;

		public void Dispose()
		{
			if (!disposedValue)
			{
				disposedValue = true;
				GC.SuppressFinalize(this);
				((IDisposable)_reader).Dispose();
			}
		}
	}
}
