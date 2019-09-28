namespace Simulation.Common
{
	public class CRC32
	{
		private static CRC32 crc = new CRC32();
		private uint[] table = null;

		private CRC32()
		{
			InitializeTable();
		}

		private uint Calculate(byte[] Data)
		{
			uint crc32 = 0xFFFFFFFF;

			for (int i = 0; i < Data.Length; ++i)
			{
				uint index = (crc32 ^ Data[i]) & 0xFF;
				crc32 = (crc32 >> 8) ^ table[index];
			}

			crc32 = crc32 ^ 0xFFFFFFFF;

			return crc32;
		}

		private void InitializeTable()
		{
			const int POLYNOMIAL = 0x04C11DB7;
			const uint TABLE_COUNT = 256;

			uint crc;
			table = new uint[TABLE_COUNT];

			for (int i = 0; i < TABLE_COUNT; ++i)
			{
				crc = (uint)i;
				for (int j = 8; j > 0; --j)
				{
					if ((crc & 1) == 1)
						crc = (crc >> 1) ^ POLYNOMIAL;
					else
						crc >>= 1;
				}

				table[i] = crc;
			}
		}

		public static uint CalculateHash(byte[] Data)
		{
			return crc.Calculate(Data);
		}
	}
}
