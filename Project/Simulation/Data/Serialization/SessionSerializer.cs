using Simulation.Data.Game;
using System;
using System.IO;

namespace Simulation.Data.Serialization
{
	public class SessionSerializer
	{
		private MemoryStream stream = null;
		private SerializerVisitor serializer = null;

		public byte[] Data
		{
			get
			{
				if (stream != null)
					return stream.ToArray();

				return null;
			}
		}

		public SessionSerializer()
		{
			stream  = new MemoryStream();
			serializer = new SerializerVisitor();
		}

		public void Finish()
		{
			stream.Close();
		}

		public void SerializeConfigState(ConfigData Config)
		{
			serializer.Reset();

			serializer.VisitInt32(Config.Seed);

			WriteInt32(serializer.Data.Length);
			WriteBuffer(serializer.Data);
		}

		public void SerializeInitialState(FrameData Frame)
		{
			SerializeFullStep(Frame);
		}

		public void SerializeStep(FrameData Frame)
		{
			serializer.Reset();

			serializer.BeginVisitArray(Frame.Events);
			if (Frame.Events != null)
				for (int i = 0; i < Frame.Events.Length; ++i)
				{
					serializer.BeginVisitArrayElement();

					Frame.Events[i].Visit(serializer);

					serializer.EndVisitArrayElement();
				}
			serializer.EndVisitArray();

			WriteInt32(Frame.Hash);
			WriteInt32(serializer.Data.Length);
			WriteBuffer(serializer.Data);
		}

		public void SerializeFullStep(FrameData Frame)
		{
			serializer.Reset();

			Frame.Visit(serializer);

			WriteInt32(Frame.Hash);
			WriteInt32(serializer.Data.Length);
			WriteBuffer(serializer.Data);
		}

		private void WriteInt32(int Value)
		{
			WriteBuffer(BitConverter.GetBytes(Value));
		}

		private void WriteBuffer(byte[] Data)
		{
			stream.Write(Data, 0, Data.Length);
			stream.Flush();
		}
	}
}
