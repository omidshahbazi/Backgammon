using System;
using System.Text;

namespace Deterministic.Common
{
	public static class DataUtility
	{
		public static string CompressData(byte[] Data)
		{
			//byte[] compressed = Compressor.Compress(Data);
			byte[] compressed = Data;

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < compressed.Length; ++i)
			{
				if (i != 0)
					builder.Append(',');

				builder.Append(compressed[i]);
			}

			return builder.ToString();
		}

		public static byte[] DecompressData(string Data)
		{
			string[] splitted = Data.Split(',');
			byte[] data = new byte[splitted.Length];

			int i = 0;
			for (; i < splitted.Length; ++i)
				data[i] = Convert.ToByte(splitted[i]);

			//return Compressor.Decompress(data);
			return data;
		}
	}
}
