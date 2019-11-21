using Simulation.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using System;
using GameFramework.BinarySerializer;

namespace Simulation.Data.Serialization
{
	public static class Deserializer
	{
		public static FrameData DeserializeFrameData(byte[] Data)
		{
			BufferStream buffer = new BufferStream(Data);

			return DeserializeFrameData(buffer);
		}

		public static BoardData DeserializeBoardData(byte[] Data)
		{
			BufferStream buffer = new BufferStream(Data);

			return DeserializeBoardData(buffer);
		}

		public static PointData DeserializePointData(byte[] Data)
		{
			BufferStream buffer = new BufferStream(Data);

			return DeserializePointData(buffer);
		}

		public static EventBase[] DeserializeEvents(byte[] Data)
		{
			BufferStream buffer = new BufferStream(Data);

			return DeserializeEventsData(buffer);
		}

		public static FrameData DeserializeFrameData(BufferStream Buffer)
		{
			FrameData data = new FrameData();

			data.Board = DeserializeBoardData(Buffer);

			data.Events = DeserializeEventsData(Buffer);

			return data;
		}

		public static EventBase[] DeserializeEventsData(BufferStream Buffer)
		{
			uint len = Buffer.BeginReadArray();

			EventBase[] events = new EventBase[len];

			for (int i = 0; i < len; ++i)
				events[i] = DeserializeEventBase(Buffer);

			return events;
		}

		public static EventBase DeserializeEventBase(BufferStream Buffer)
		{
			EventBase.Types type = (EventBase.Types)Buffer.ReadInt32();

			switch (type)
			{
				case EventBase.Types.BoardToBoardMove:
					return DeserializeBoardToBoardMoveEvent(Buffer);

				case EventBase.Types.BarToBoardMove:
					return DeserializeBarToBoardMoveEvent(Buffer);

				case EventBase.Types.BearOff:
					return DeserializeBearedOffEvent(Buffer);

				case EventBase.Types.FinishTurn:
					return DeserializeFinishTurnEvent(Buffer);

				default:
					throw new Exception("Unsupported Type");
			}
		}

		private static void DeserializeDataBase(BufferStream Buffer, DataBase Data)
		{
		}

		private static BoardToBoardMoveEvent DeserializeBoardToBoardMoveEvent(BufferStream Buffer)
		{
			Identifier from = ReadIdentifier(Buffer);
			Identifier to = ReadIdentifier(Buffer);

			return new BoardToBoardMoveEvent(from, to);
		}

		private static BarToBoardMoveEvent DeserializeBarToBoardMoveEvent(BufferStream Buffer)
		{
			PlayerColors color = (PlayerColors)Buffer.ReadInt32();
			Identifier to = ReadIdentifier(Buffer);

			return new BarToBoardMoveEvent(color, to);
		}

		private static BearOffEvent DeserializeBearedOffEvent(BufferStream Buffer)
		{
			Identifier from = ReadIdentifier(Buffer);

			return new BearOffEvent(from);
		}

		private static FinishTurnEvent DeserializeFinishTurnEvent(BufferStream Buffer)
		{
			PlayerColors color = (PlayerColors)Buffer.ReadInt32();

			return new FinishTurnEvent(color);
		}

		public static BoardData DeserializeBoardData(BufferStream Buffer)
		{
			BoardData data = new BoardData();

			DeserializeDataBase(Buffer, data);

			uint len = Buffer.BeginReadArray();
			data.Points = new PointData[len];

			for (int i = 0; i < len; ++i)
				data.Points[i] = DeserializePointData(Buffer);

			data.WhitePlayer = DeserializePlayerData(Buffer);
			data.BlackPlayer = DeserializePlayerData(Buffer);

			data.TurnColor = (PlayerColors)Buffer.ReadInt32();

			data.TurnDice = DeserializeDiceData(Buffer);

			data.TurnNumber = Buffer.ReadInt32();

			return data;
		}

		public static PlayerData DeserializePlayerData(BufferStream Buffer)
		{
			PlayerData data = new PlayerData();

			DeserializeDataBase(Buffer, data);

			data.InitialDice = DeserializeDiceData(Buffer);

			data.Color = (PlayerColors)Buffer.ReadInt32();

			data.BarCheckerCount = Buffer.ReadInt32();
			data.BearedOffCheckersCount = Buffer.ReadInt32();

			data.MoveCount = Buffer.ReadInt32();

			return data;
		}

		public static DiceData DeserializeDiceData(BufferStream Buffer)
		{
			DiceData data = new DiceData();

			DeserializeDataBase(Buffer, data);

			uint len = Buffer.BeginReadArray();
			data.Moves = new int[len];

			for (int i = 0; i < len; ++i)
				data.Moves[i] = Buffer.ReadInt32();

			data.IsPair = Buffer.ReadBool();

			return data;
		}

		public static PointData DeserializePointData(BufferStream Buffer)
		{
			PointData data = new PointData();

			DeserializeDataBase(Buffer, data);

			data.ID = ReadIdentifier(Buffer);
			data.Index = Buffer.ReadInt32();
			data.CheckerCount = Buffer.ReadInt32();
			data.Color = (PlayerColors)Buffer.ReadInt32();

			return data;
		}

		public static Identifier ReadIdentifier(BufferStream Buffer)
		{
			return new Identifier(Buffer.ReadInt32());
		}
	}
}
