#define ENABLE_COMPRESSION
//#define ENABLE_ENCRYPTION
using GameFramework.BinarySerializer;
using GameFramework.Common.Compression;
using System;
using System.Diagnostics;

namespace Networking.Common
{
	public static class NetworkingCommon
	{
		private const byte IS_COMPRESSED = 1;
		private const byte IS_ENCRYPTED = 1;
		private const float RATIO_MULTIPLIER = 120;

		// IS_COMPRESSED | IS_ENCRYPTED | RATIO | DATA_SIZE
		private const uint HEADER_SIZE = 7;

		public static BufferStream PrepareForSend(BufferStream Buffer)
		{
			uint bufferSize = Buffer.Size;

			byte[] buffer = new byte[HEADER_SIZE + (int)(bufferSize * 1.5F)];

			bool isCompressed = (bufferSize > 32);
			bool isEncrypted = false;

#if !ENABLE_COMPRESSION
			isCompressed = false;
#endif
#if !ENABLE_ENCRYPTION
			isEncrypted = false;
#endif

			buffer[0] = (isCompressed ? IS_COMPRESSED : (byte)0);
			buffer[1] = (isEncrypted ? IS_ENCRYPTED : (byte)0);

			if (isCompressed)
			{
				int len = Compressor.Compress(Buffer.Buffer, 0, bufferSize, buffer, HEADER_SIZE, (uint)buffer.Length);

				Debug.Assert(len != 0, "Compression has failed");

				buffer[2] = (byte)(((bufferSize / (float)len) - 1) * RATIO_MULTIPLIER);

				bufferSize = (uint)len;
			}
			else
				Array.Copy(Buffer.Buffer, 0, buffer, HEADER_SIZE, bufferSize);

			Array.Copy(BitConverter.GetBytes(bufferSize), 0, buffer, 3, sizeof(int));

			return new BufferStream(buffer, HEADER_SIZE + bufferSize);
		}

		public static BufferStream PrepareForReceive(BufferStream Buffer)
		{
			uint bufferSize = Buffer.Size;

			bool isCompressed = (Buffer.ReadByte() == IS_COMPRESSED);
			bool isEncrypted = (Buffer.ReadByte() == IS_ENCRYPTED);
			byte ratio = Buffer.ReadByte();
			uint dataSize = Buffer.ReadUInt32();
			float multiplier = 1 + (ratio / RATIO_MULTIPLIER);

#if !ENABLE_COMPRESSION
			isCompressed = false;
#endif
#if !ENABLE_ENCRYPTION
			isEncrypted = false;
#endif

			byte[] buffer = new byte[(int)(bufferSize * multiplier)];

			if (isCompressed)
			{
				int len = Compressor.Decompress(Buffer.Buffer, HEADER_SIZE, dataSize, buffer, 0, (uint)buffer.Length);

				Debug.Assert(len != 0, "Decompression has failed");

				dataSize = (uint)len;
			}
			else
				Array.Copy(Buffer.Buffer, HEADER_SIZE, buffer, 0, dataSize);

			return new BufferStream(buffer, dataSize);
		}
	}
}