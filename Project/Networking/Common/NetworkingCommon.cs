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
		private const uint HEADER_SIZE = 1 + 1 + sizeof(uint) + sizeof(uint);

		public static BufferStream PrepareForSend(BufferStream Buffer)
		{
			uint originalDataSize = Buffer.Size;

			byte[] buffer = new byte[HEADER_SIZE + (int)(originalDataSize * 1.5F)];

			bool isCompressed = (originalDataSize > 32);
			bool isEncrypted = false;

#if !ENABLE_COMPRESSION
			isCompressed = false;
#endif
#if !ENABLE_ENCRYPTION
			isEncrypted = false;
#endif

			int index = 0;
			buffer[index++] = (isCompressed ? IS_COMPRESSED : (byte)0);
			buffer[index++] = (isEncrypted ? IS_ENCRYPTED : (byte)0);

			uint processedDataSize = originalDataSize;
			if (isCompressed)
			{
				processedDataSize = (uint)Compressor.Compress(Buffer.Buffer, 0, originalDataSize, buffer, HEADER_SIZE, (uint)buffer.Length);

				Debug.Assert(processedDataSize != 0, "Compression has failed");
			}
			else
				Array.Copy(Buffer.Buffer, 0, buffer, HEADER_SIZE, originalDataSize);

			Array.Copy(BitConverter.GetBytes(processedDataSize), 0, buffer, index, sizeof(int));
			index += sizeof(uint);

			Array.Copy(BitConverter.GetBytes(originalDataSize), 0, buffer, index, sizeof(int));
			index += sizeof(uint);

			return new BufferStream(buffer, HEADER_SIZE + processedDataSize);
		}

		public static BufferStream PrepareForReceive(BufferStream Buffer)
		{
			bool isCompressed = (Buffer.ReadByte() == IS_COMPRESSED);
			bool isEncrypted = (Buffer.ReadByte() == IS_ENCRYPTED);
			uint processedDataSize = Buffer.ReadUInt32();
			uint originalDataSize = Buffer.ReadUInt32();

#if !ENABLE_COMPRESSION
			isCompressed = false;
#endif
#if !ENABLE_ENCRYPTION
			isEncrypted = false;
#endif

			byte[] buffer = new byte[(int)(originalDataSize)];

			if (isCompressed)
			{
				int len = Compressor.Decompress(Buffer.Buffer, HEADER_SIZE, processedDataSize, buffer, 0, (uint)buffer.Length);

				Debug.Assert(len != 0, "Decompression has failed");
			}
			else
				Array.Copy(Buffer.Buffer, HEADER_SIZE, buffer, 0, originalDataSize);

			return new BufferStream(buffer, originalDataSize);
		}
	}
}