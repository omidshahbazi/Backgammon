using GameFramework.BinarySerializer;
using GameFramework.Common.Compression;
using System;

namespace Networking.Common
{
	public static class NetworkingCommon
	{
		private const byte IS_COMPRESSED = 1;
		private const byte IS_ENCRYPTED = 1;

		private const uint HEADER_SIZE = 3;

		public static BufferStream PrepareForSend(BufferStream Buffer)
		{
			uint bufferSize = Buffer.Size;

			byte[] buffer = new byte[HEADER_SIZE + bufferSize];

			bool isCompressed = (bufferSize > 10000);
			bool isEncrypted = false;

			buffer[0] = (isCompressed ? IS_COMPRESSED : (byte)0);
			buffer[1] = (isEncrypted ? IS_ENCRYPTED : (byte)0);

			if (isCompressed)
			{
				int len = Compressor.Compress(Buffer.Buffer, 0, bufferSize, buffer, HEADER_SIZE, bufferSize);

				buffer[2] = (byte)(((bufferSize / (float)len) - 1) * 100);

				bufferSize = (uint)len;
			}
			else
				Array.Copy(Buffer.Buffer, 0, buffer, HEADER_SIZE, bufferSize);

			return new BufferStream(buffer, HEADER_SIZE + bufferSize);
		}

		public static BufferStream PrepareForReceive(BufferStream Buffer)
		{
			uint bufferSize = Buffer.Size;

			bool isCompressed = (Buffer.ReadByte() == IS_COMPRESSED);
			bool isEncrypted = (Buffer.ReadByte() == IS_ENCRYPTED);
			byte ratio = Buffer.ReadByte();
			float multiplier = 1 + (ratio / 100.0F);

			byte[] buffer = new byte[(int)(bufferSize * multiplier)];

			uint dataSize = bufferSize - HEADER_SIZE;

			if (isCompressed)
				Compressor.Decompress(Buffer.Buffer, HEADER_SIZE, dataSize, buffer, 0, (uint)buffer.Length);
			else
				Array.Copy(Buffer.Buffer, HEADER_SIZE, buffer, 0, dataSize);

			return new BufferStream(buffer, dataSize);
		}
	}
}