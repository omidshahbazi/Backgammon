using Simulation.Common;
using Simulation.Common.Serialization;
using Simulation.Data.Game;
using System.IO;

namespace Simulation.Data.Serialization
{
	public class SessionDeserializer
	{
		private MemoryStream stream = null;

		public byte[] Data
		{
			get
			{
				byte[] data = new byte[stream.Length];
				stream.Read(data, 0, data.Length);
				return data;
			}
		}

		public SessionDeserializer(byte[] Data)
		{
			stream = new MemoryStream(Data, false);
		}

		public void Reset()
		{
			stream.Seek(0, SeekOrigin.Begin);
		}

		public ConfigData DeserializeConfigDataState()
		{
			int dataLength = ReadInt32();

			byte[] data = ReadBuffer(dataLength);
			Serializer serializer = new Serializer(new MemoryStream(data, 0, data.Length, false, true));

			ConfigData config = new ConfigData();
			config.Seed = serializer.ReadInt32();
			config.Random = new Random(config.Seed);

			return config;
		}

		public FrameData DeserializeInitialState()
		{
			int framaHash = ReadInt32();
			int dataLength = ReadInt32();
			FrameData frame = Deserializer.DeserializeFrameData(ReadBuffer(dataLength));
			frame.Hash = framaHash;
			return frame;
		}

		public FrameData DeserializeStep()
		{
			int framaHash = ReadInt32();
			int dataLength = ReadInt32();

			if (dataLength == 0)
				return null;

			FrameData frame = new FrameData();
			frame.Hash = framaHash;
			frame.Events = Deserializer.DeserializeEvents(ReadBuffer(dataLength));
			return frame;
		}

		public FrameData DeserializeFullStep()
		{
			int framaHash = ReadInt32();
			int dataLength = ReadInt32();

			if (dataLength == 0)
				return null;

			FrameData frame = Deserializer.DeserializeFrameData(ReadBuffer(dataLength));
			frame.Hash = framaHash;
			return frame;
		}

		private int ReadInt32()
		{
			return System.BitConverter.ToInt32(ReadBuffer(4), 0);
		}

		private byte[] ReadBuffer(int Length)
		{
			byte[] data = new byte[Length];
			stream.Read(data, 0, Length);
			return data;
		}
	}
}
