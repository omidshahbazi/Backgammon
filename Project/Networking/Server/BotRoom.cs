using Networking.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using System.Diagnostics;

namespace Networking.Server
{
	class BotRoom : RoomBase
	{
		public BotRoom(Application Application, int GameID) :
			base(Application, GameID)
		{
		}

		protected override void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			Simulator.SendEvent(Event);

			Debug.Assert(ClientHash == Simulator.Hash);

			if (Event.GetType() == EventBase.Types.FinishTurn)
			{
				FindPointAndMove(Simulator.Board.TurnDice.Dice1);
				FindPointAndMove(Simulator.Board.TurnDice.Dice2);

				if (Simulator.Board.TurnDice.Dice1 == Simulator.Board.TurnDice.Dice2)
				{
					FindPointAndMove(Simulator.Board.TurnDice.Dice1);
					FindPointAndMove(Simulator.Board.TurnDice.Dice2);
				}

				FinishTurn();
			}
		}

		private void FindPointAndMove(int Dice)
		{
			PointData fromPoint = null;
			PointData toPoint = null;
			if (!GetFirstPossibleMove(Dice, out fromPoint, out toPoint))
				return;

			Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, toPoint.ID));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.BOARD_TO_BOARD_MOVE);
			SendBuffer.WriteInt32(Simulator.Hash);
			SendBuffer.WriteInt32(fromPoint.ID);
			SendBuffer.WriteInt32(toPoint.ID);

			SendToAll(SendBuffer);
		}

		private void FinishTurn()
		{
			PlayerColors color = Simulator.Board.TurnColor;
			Simulator.SendEvent(new FinishTurnEvent(color));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			SendBuffer.WriteInt32(Simulator.Hash);
			SendBuffer.WriteInt32((int)color);

			SendToAll(SendBuffer);
		}

		private bool GetFirstPossibleMove(int Dice, out PointData FromPoint, out PointData ToPoint)
		{
			FromPoint = null;
			ToPoint = null;

			for (int i = 0; i < Simulator.Board.Points.Length; ++i)
			{
				PointData fromPoint = Simulator.Board.Points[i];

				PointData[] targetPoints = Logic.GetPossibleMoves(Simulator.Board, fromPoint.ID, Dice);
				if (targetPoints == null || targetPoints.Length == 0)
					continue;

				FromPoint = fromPoint;
				ToPoint = targetPoints[Configs.Random.Next(0, targetPoints.Length)];

				return true;
			}

			return false;
		}
	}
}