using Networking.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;

namespace Networking.Server
{
	class OneByBotRoom : Room
	{
		private string botPlayerInfo;

		protected override Player WhitePlayer
		{
			get { return Players[0]; }
		}

		protected override Player BlackPlayer
		{
			get { return null; }
		}

		public override string BotPlayerInfo
		{
			get { return botPlayerInfo; }
		}

		public OneByBotRoom(Application Application, uint TableEnterance) :
			base(Application, TableEnterance)
		{
		}

		public override void Initialize()
		{
			botPlayerInfo = BotPlayerInfoMaker.Make(WhitePlayer.ID);

			base.Initialize();
		}

		protected override void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			Simulator.SendEvent(Event);

			SerializeStep();

			if (ClientHash != Simulator.Frame.Hash)
			{
				HandleGameFinisher(Player, GameFinishReasons.Mismatch);

				return;
			}

			if (Event.GetType() == EventBase.Types.FinishTurn)
			{
				FindPointAndMove(Simulator.Frame.Board.TurnDice.Dice1);
				FindPointAndMove(Simulator.Frame.Board.TurnDice.Dice2);

				if (Simulator.Frame.Board.TurnDice.Dice1 == Simulator.Frame.Board.TurnDice.Dice2)
				{
					FindPointAndMove(Simulator.Frame.Board.TurnDice.Dice1);
					FindPointAndMove(Simulator.Frame.Board.TurnDice.Dice2);
				}

				FinishTurn();
			}
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Enterance);
		}

		protected override void HandleGetGameData(Player Player)
		{
			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			SendBuffer.WriteInt32((int)PlayerColors.White);

			Send(Player, SendBuffer);
		}

		protected override void AddWinnerReward(Player WinnerPlayer, RewardInfo Reward)
		{
			DatabaseLayer.AddReward(WinnerPlayer.ID, Reward);
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
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32(fromPoint.ID);
			SendBuffer.WriteInt32(toPoint.ID);

			SendToAll(SendBuffer);
		}

		private void FinishTurn()
		{
			PlayerColors color = Simulator.Frame.Board.TurnColor;
			Simulator.SendEvent(new FinishTurnEvent(color));

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.FINISH_TURN);
			SendBuffer.WriteInt32(Simulator.Frame.Hash);
			SendBuffer.WriteInt32((int)color);

			SendToAll(SendBuffer);
		}

		private bool GetFirstPossibleMove(int Dice, out PointData FromPoint, out PointData ToPoint)
		{
			FromPoint = null;
			ToPoint = null;

			for (int i = 0; i < Simulator.Frame.Board.Points.Length; ++i)
			{
				PointData fromPoint = Simulator.Frame.Board.Points[i];

				PointData[] targetPoints = Logic.GetPossibleMoves(Simulator.Frame.Board, fromPoint.ID, Dice);
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