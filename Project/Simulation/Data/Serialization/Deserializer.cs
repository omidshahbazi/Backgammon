using Simulation.Common;
using Simulation.Common.Serialization;
using Simulation.Data.Event;
using Simulation.Data.Game;
using System;
using System.IO;

namespace Simulation.Data.Serialization
{
	public static class Deserializer
	{
		public static FrameData DeserializeFrameData(byte[] Data)
		{
			Serializer serializer = new Serializer(new MemoryStream(Data, 0, Data.Length, false, true));

			return DeserializeFrameData(serializer);
		}

		public static BoardData DeserializeBoardData(byte[] Data)
		{
			Serializer serializer = new Serializer(new MemoryStream(Data, 0, Data.Length, false, true));

			return DeserializeBoardData(serializer);
		}

		public static PointData DeserializePointData(byte[] Data)
		{
			Serializer serializer = new Serializer(new MemoryStream(Data, 0, Data.Length, false, true));

			return DeserializePointData(serializer);
		}

		public static EventBase[] DeserializeEvents(byte[] Data)
		{
			Serializer serializer = new Serializer(new MemoryStream(Data, 0, Data.Length, false, true));

			return DeserializeEventsData(serializer);
		}

		public static FrameData DeserializeFrameData(Serializer Serializer)
		{
			FrameData data = new FrameData();

			data.Board = DeserializeBoardData(Serializer);

			data.Events = DeserializeEventsData(Serializer);

			return data;
		}

		public static EventBase[] DeserializeEventsData(Serializer Serializer)
		{
			int len = Serializer.BeginReadArray();

			EventBase[] events = new EventBase[len];

			for (int i = 0; i < len; ++i)
				events[i] = DeserializeEventBase(Serializer);

			return events;
		}

		public static EventBase DeserializeEventBase(Serializer Serializer)
		{
			EventBase.Types type = (EventBase.Types)Serializer.ReadInt32();

			switch (type)
			{
				case EventBase.Types.Move:
					return DeserializeMoveEvent(Serializer);

				default:
					throw new Exception("Unsupported Type");
			}
		}

		private static void DeserializeDataBase(Serializer Serializer, DataBase Data)
		{
		}

		private static MoveEvent DeserializeMoveEvent(Serializer Serializer)
		{
			Identifier from = ReadIdentifier(Serializer);
			Identifier to = ReadIdentifier(Serializer);

			return new MoveEvent(from, to);
		}

		public static BoardData DeserializeBoardData(Serializer Serializer)
		{
			BoardData data = new BoardData();

			DeserializeDataBase(Serializer, data);

			int len = Serializer.BeginReadArray();
			data.Points = new PointData[len];

			for (int i = 0; i < len; ++i)
				data.Points[i] = DeserializePointData(Serializer);

			data.TurnColor = (PlayerColors)Serializer.ReadInt32();
			data.Dice1 = Serializer.ReadInt32();
			data.Dice2 = Serializer.ReadInt32();

			data.OnBarWhiteCheckerCount = Serializer.ReadInt32();
			data.OnBarBlackCheckerCount = Serializer.ReadInt32();
			data.BearedOffWhiteCheckersCount = Serializer.ReadInt32();
			data.BearedOffBlackCheckersCount = Serializer.ReadInt32();

			return data;
		}

		public static PointData DeserializePointData(Serializer Serializer)
		{
			PointData data = new PointData();

			DeserializeDataBase(Serializer, data);

			data.CheckerCount = Serializer.ReadInt32();
			data.Color = (PlayerColors)Serializer.ReadInt32();

			return data;
		}

		public static Identifier ReadIdentifier(Serializer Serializer)
		{
			return new Identifier(Serializer.ReadInt32());
		}
	}
}
