using GameFramework.Common.Utilities;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using System.Collections.Generic;

namespace Simulation.Logic
{
	//https://en.wikipedia.org/wiki/Expectiminimax
	public static class SmartBotUtilities
	{
		private enum Nodes
		{
			Max, Min, Chance
		}

		private static readonly Nodes[] NODES = new Nodes[] { Nodes.Max, Nodes.Chance, Nodes.Min, Nodes.Chance };
		private const float HEURISTIC_MULTIPLIER = .055F;

		public static readonly int[][] DICE_PAIRS = null;

		static SmartBotUtilities()
		{
			List<int[]> pairs = new List<int[]>();

			for (int i = ConfigData.MIN_DICE_NUMBER; i <= ConfigData.MAX_DICE_NUMBER; ++i)
				for (int j = i; j <= ConfigData.MAX_DICE_NUMBER; ++j)
					pairs.Add(new int[] { i, j });

			DICE_PAIRS = pairs.ToArray();
		}

		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			while (Player.MoveCount != 0)
			{
				List<MoveInfo> possibleMoves = new List<MoveInfo>();
				GetBoardToBoard(board, possibleMoves);
				GetPlayBarToBoard(board, Player, possibleMoves);
				GetPlayBearOff(board, Player, possibleMoves);

				List<float> moveQuality = new List<float>();
				ExpectiMiniMax(Simulator.Frame.Board, Player, possibleMoves.ToArray(), moveQuality, 3, 0);

				float maxQuality = MathUtilities.Max(moveQuality);
				int moveIndex = moveQuality.IndexOf(maxQuality);

				if (moveIndex == -1)
					continue;

				MoveInfo move = possibleMoves[moveIndex];

				EventBase ev = null;

				if (move.From != null && move.To != null)
					ev = new BoardToBoardMoveEvent(move.From.ID, move.To.ID);
				else if (move.From != null)
					ev = new BearOffEvent(move.From.ID);
				else if (move.To != null)
					ev = new BarToBoardMoveEvent(Player.Color, move.To.ID);

				Simulator.SendEvent(ev);

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}
			}
		}

		private static float ExpectiMiniMax(BoardData Board, PlayerData Player, MoveInfo[] PossibleMoves, List<float> MoveQuality, int Depth, int NodeIndex, int InitialDepth = -1)
		{
			float result = 0;

			switch (NODES[NodeIndex])
			{
				case Nodes.Max:
					{
						if (InitialDepth == -1)
							InitialDepth = Depth;

						if (PossibleMoves != null)
						{
							float[] moveQuality = new float[PossibleMoves.Length];

							for (uint i = 0; i < PossibleMoves.Length; ++i)
							{
								MoveInfo move = PossibleMoves[i];

								//make(move);

								moveQuality[i] = ExpectiMiniMax(Board, Player, PossibleMoves, MoveQuality, Depth - 1, (NodeIndex + 1) % 4, InitialDepth);

								//unmake(move);
							}

							if (Depth == InitialDepth)
							{
								MoveQuality.Clear();
								MoveQuality.AddRange(moveQuality);
							}

							result = MathUtilities.Max(moveQuality);
						}
						else
						{
							if (Depth == InitialDepth)
								MoveQuality.Clear();

							result = -0.9F;
						}
					}
					break;
				case Nodes.Min:
					{
						if (PossibleMoves != null)
						{
							float[] moveQuality = new float[PossibleMoves.Length];

							for (uint i = 0; i < PossibleMoves.Length; ++i)
							{
								MoveInfo move = PossibleMoves[i];

								//opponent.make(move);

								moveQuality[i] = ExpectiMiniMax(Board, Player, PossibleMoves, MoveQuality, Depth - 1, (NodeIndex + 1) % 4, InitialDepth);

								//opponent.unmake(move);
							}

							result = MathUtilities.Min(moveQuality);
						}
						else
						{
							result = 0.9F;
						}
					}
					break;
				case Nodes.Chance:
					{
						if (Depth == 0)
							result = HeuristicValue(Board, Player, NodeIndex - 1);
						else
						{
							List<float> values = new List<float>();

							for (uint i = 0; i < DICE_PAIRS.Length; i += 2)
							{
								//int[] currentDiceMove = null;

								//if (NODES[(NodeIndex + 1) % 4] == Nodes.Max)
								//{

								//	setDice(DICE_PAIRS[i], DICE_PAIRS[i + 1]);
								//}
								//else
								//{
								//	//currentDice = opponent.getDice();
								//	//opponent.setDice(DICE_PAIRS[i], DICE_PAIRS[i + 1]);
								//}

								values.Add(ExpectiMiniMax(Board, Player, PossibleMoves, MoveQuality, Depth - 1, (NodeIndex + 1) % 4));

								//if (NODES[(NodeIndex + 1) % 4] == Nodes.Max)
								//{
								//	setDice(currentDice);
								//}
								//else
								//{
								//	opponent.setDice(currentDice);
								//}
							}

							result = WeightedAverage(values);
						}
					}
					break;
			}

			return result;
		}

		private static float HeuristicValue(BoardData Board, PlayerData Player, int NodeIndex)
		{
			int playerCheckerOnBarCount = Player.BarCheckerCount;
			int opponentChceckerOnBarCount = Utilities.GetOpponentPlayer(Board, Player.Color).BarCheckerCount;

			if (NODES[NodeIndex] == Nodes.Max)
				return HEURISTIC_MULTIPLIER * (opponentChceckerOnBarCount - playerCheckerOnBarCount);

			return -HEURISTIC_MULTIPLIER * (playerCheckerOnBarCount - opponentChceckerOnBarCount);
		}

		private static float WeightedAverage(List<float> Values)
		{
			float weightedSum = 0;

			float coefficientSum = 0;

			for (uint i = 0; i < Values.Count; ++i)
			{
				float multiplier = GetDiceProbability(DICE_PAIRS[i][0], DICE_PAIRS[i][1]);

				weightedSum += Values[(int)i] * multiplier;

				coefficientSum += multiplier;
			}

			return weightedSum / coefficientSum;
		}

		private static float GetDiceProbability(int Dice1, int Dice2)
		{
			return 1.0F / (Dice1 == Dice2 ? (ConfigData.MAX_DICE_NUMBER * ConfigData.MAX_DICE_NUMBER) : (ConfigData.MAX_DICE_NUMBER * 2));
		}

		public static void GetBoardToBoard(BoardData Board, List<MoveInfo> Moves)
		{
			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = Board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(Board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Moves.AddRange(moves);
			}
		}

		public static void GetPlayBarToBoard(BoardData Board, PlayerData Player, List<MoveInfo> Moves)
		{
			if (Player.BarCheckerCount == 0)
				return;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(Board);

			if (moves == null || moves.Length == 0)
				return;

			Moves.AddRange(moves);
		}

		public static void GetPlayBearOff(BoardData Board, PlayerData Player, List<MoveInfo> Moves)
		{
			if (Utilities.GetInBaseCheckerCount(Board.Points, Board.TurnColor) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return;

			for (int i = 0; i < ConfigData.POINT_COUNT && Player.MoveCount != 0; ++i)
			{
				PointData fromPoint = Board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBearedOffs(Board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Moves.AddRange(moves);
			}
		}
	}
}
