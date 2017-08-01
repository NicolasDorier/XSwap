using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace XSwap.CLI
{
    public static class Utils
    {
		public static byte[] Zip(string unzipped)
		{
			MemoryStream ms = new MemoryStream();
			using(GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
			{
				StreamWriter writer = new StreamWriter(gzip, Encoding.UTF8);
				writer.Write(unzipped);
				writer.Flush();
			}
			return ms.ToArray();
		}
		public static string Unzip(byte[] bytes)
		{
			MemoryStream ms = new MemoryStream(bytes);
			using(GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
			{
				StreamReader reader = new StreamReader(gzip, Encoding.UTF8);
				var unzipped = reader.ReadToEnd();
				return unzipped;
			}
		}
		public static void DeleteRecursivelyWithMagicDust(string destinationDir)
		{
			const int magicDust = 10;
			for(var gnomes = 1; gnomes <= magicDust; gnomes++)
			{
				try
				{
					Directory.Delete(destinationDir, true);
				}
				catch(DirectoryNotFoundException)
				{
					return;  // good!
				}
				catch(IOException)
				{
					if(gnomes == magicDust)
						throw;
					// System.IO.IOException: The directory is not empty
					System.Diagnostics.Debug.WriteLine("Gnomes prevent deletion of {0}! Applying magic dust, attempt #{1}.", destinationDir, gnomes);

					// see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true for more magic
					Thread.Sleep(100);
					continue;
				}
				catch(UnauthorizedAccessException)
				{
					if(gnomes == magicDust)
						throw;
					// Wait, maybe another software make us authorized a little later
					System.Diagnostics.Debug.WriteLine("Gnomes prevent deletion of {0}! Applying magic dust, attempt #{1}.", destinationDir, gnomes);

					// see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true for more magic
					Thread.Sleep(100);
					continue;
				}
				return;
			}
			// depending on your use case, consider throwing an exception here
		}
	}
}
