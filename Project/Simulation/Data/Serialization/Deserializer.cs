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
				case EventBase.Types.BoardToBoardMove:
					return DeserializeBoardToBoardMoveEvent(Serializer);

				case EventBase.Types.BearOff:
					return DeserializeBarToBoardMoveEvent(Serializer);

				case EventBase.Types.BearedOff:
					return DeserializeBearedOffEvent(Serializer);

				case EventBase.Types.FinishTurn:
					return DeserializeFinishTurnEvent(Serializer);

				default:
					throw new Exception("Unsupported Type");
			}
		}

		private static void DeserializeDataBase(Serializer Serializer, DataBase Data)
		{
		}

		private static BoardToBoardMoveEvent DeserializeBoardToBoardMoveEvent(Serializer Serializer)
		{
			Identifier from = ReadIdentifier(Serializer);
			Identifier to = ReadIdentifier(Serializer);

			return new BoardToBoardMoveEvent(from, to);
		}

		private static BarToBoardMoveEvent DeserializeBarToBoardMoveEvent(Serializer Serializer)
		{
			PlayerColors color = (PlayerColors)Serializer.ReadInt32();
			Identifier to = ReadIdentifier(Serializer);

			return new BarToBoardMoveEvent(color, to);
		}

		private static BearOffEvent DeserializeBearedOffEvent(Serializer Serializer)
		{
			Identifier from = ReadIdentifier(Serializer);

			return new BearOffEvent(from);
		}

		private static FinishTurnEvent DeserializeFinishTurnEvent(Serializer Serializer)
		{
			PlayerColors color = (PlayerColors)Serializer.ReadInt32();

			return new FinishTurnEvent(color);
		}

		public static BoardData DeserializeBoardData(Serializer Serializer)
		{
			BoardData data = new BoardData();

			DeserializeDataBase(Serializer, data);

			int len = Serializer.BeginReadArray();
			data.Points = new PointData[len];

			for (int i = 0; i < len; ++i)
				data.Points[i] = DeserializePointData(Serializer);

			data.WhitePlayer = DeserializePlayerData(Serializer);
			data.BlackPlayer = DeserializePlayerData(Serializer);

			data.TurnColor = (PlayerColors)Serializer.ReadInt32();

			data.TurnDice = DeserializeDiceData(Serializer);

			return data;
		}

		public static PlayerData DeserializePlayerData(Serializer Serializer)
		{
			PlayerData data = new PlayerData();

			DeserializeDataBase(Serializer, data);

			data.InitialDice = DeserializeDiceData(Serializer);

			data.Color = (PlayerColors)Serializer.ReadInt32();

			data.BarCheckerCount = Serializer.ReadInt32();
			data.BearedOffCheckersCount = Serializer.ReadInt32();

			data.MoveCount = Serializer.ReadInt32();

			return data;
		}

		public static DiceData DeserializeDiceData(Serializer Serializer)
		{
			DiceData data = new DiceData();

			DeserializeDataBase(Serializer, data);

			data.Dice1 = Serializer.ReadInt32();
			data.Dice2 = Serializer.ReadInt32();

			return data;
		}

		public static PointData DeserializePointData(Serializer Serializer)
		{
			PointData data = new PointData();

			DeserializeDataBase(Serializer, data);

			data.ID = ReadIdentifier(Serializer);
			data.Index = Serializer.ReadInt32();
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
