using Simulation.Common;
using Simulation.Data.Game;
using GameFramework.BinarySerializer;
using GameFramework.Common.Utilities;

namespace Simulation.Data.Serialization
{
	public class SessionDeserializer
	{
		private BufferStream buffer = null;

		public byte[] Data
		{
			get { return buffer.Buffer; }
		}

		public SessionDeserializer(byte[] Data)
		{
			buffer = new BufferStream(Data);
		}

		public void Reset()
		{
			buffer.Reset();
		}

		public ConfigData DeserializeConfigDataState()
		{
			int dataLength = ReadInt32();

			byte[] data = ReadBuffer(dataLength);
			BufferStream serializer = new BufferStream(data);

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
			buffer.ReadBytes(data, 0, Length);
			return data;
		}
	}
}
