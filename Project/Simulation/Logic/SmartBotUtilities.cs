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
		private static double coefficient;
		private const float HEURISTIC_MULTIPLIER = .055F;

		public static void PlayOneTurn(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			MoveInfo[] possibleMoves = new MoveInfo[1];

			List<float> moveQuality = null;
			ExpectiMiniMax(Simulator, Player, possibleMoves, moveQuality, 3, 0);

			while (Player.MoveCount != 0)
			{
				PlayBarToBoard(Simulator, Random, Player, Serializer, FullStep);

				PlayBearOff(Simulator, Player, Serializer, FullStep);

				PlayBoardToBoard(Simulator, Random, Serializer, FullStep);
			}
		}

		private static float ExpectiMiniMax(Simulator Simulator, PlayerData Player, MoveInfo[] PossibleMoves, List<float> MoveQuality, int Depth, int NodeIndex, int InitialDepth = -1)
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

								moveQuality[i] = ExpectiMiniMax(Simulator, Player, PossibleMoves, MoveQuality, Depth - 1, (NodeIndex + 1) % 4, InitialDepth);

								//unmake(move);
							}

							if (Depth == InitialDepth)
							{
								MoveQuality = new List<float>();
								MoveQuality.AddRange(moveQuality);
							}

							result = MathUtilities.Max(moveQuality);
						}
						else
						{
							if (Depth == InitialDepth)
								MoveQuality = null;

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

								moveQuality[i] = ExpectiMiniMax(Simulator, Player, PossibleMoves, MoveQuality, Depth - 1, (NodeIndex + 1) % 4, InitialDepth);

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
						{
							result = HeuristicValue(Simulator, Player, NodeIndex - 1);
						}
						else
						{
							Die[] dice = Die.DICE_PAIRS;
							List<float> values = new ArrayList<>();
							for (uint i = 0; i < dice.Count; i += 2)
							{
								Die[] currentDice;
								if (NODES[(NodeIndex + 1) % 4] == Node.MAX)
								{
									currentDice = getDice();
									setDice(dice[i], dice[i + 1]);
								}
								else
								{
									currentDice = opponent.getDice();
									opponent.setDice(dice[i], dice[i + 1]);
								}
								values.add(expectiminimax(depth - 1, (NodeIndex + 1) % 4));
								if (NODES[(NodeIndex + 1) % 4] == Node.MAX)
								{
									setDice(currentDice);
								}
								else
								{
									opponent.setDice(currentDice);
								}
							}
							result = WeightedAverage(values);
						}
					}
					break;
			}

			return result;
		}

		private static float HeuristicValue(Simulator Simulator, PlayerData Player, int NodeIndex)
		{
			int playerCheckerOnBarCount = Player.BarCheckerCount;
			int opponentChceckerOnBarCount = Utilities.GetOpponentPlayer(Simulator.Frame.Board, Player.Color).BarCheckerCount;

			if (NODES[NodeIndex] == Nodes.Max)
				return HEURISTIC_MULTIPLIER * (opponentChceckerOnBarCount - playerCheckerOnBarCount);

			return -HEURISTIC_MULTIPLIER * (playerCheckerOnBarCount - opponentChceckerOnBarCount);
		}

		private static double WeightedAverage(List<float> Values)
		{
			double weightedSum = 0;

			double coefficientSum = 0;

			int diceIndex = 0;
			for (uint i = 0; i < Values.Count; ++i)
			{
				double multiplier = Die.probability(Die.DICE_PAIRS[diceIndex++], Die.DICE_PAIRS[diceIndex++]);

				weightedSum += Values[(int)i] * multiplier;

				coefficientSum += multiplier;
			}

			return weightedSum / coefficientSum;
		}

		public static bool PlayBoardToBoard(Simulator Simulator, Random Random, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			for (int i = 0; i < ConfigData.POINT_COUNT; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBoardToBoardMoves(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BoardToBoardMoveEvent(fromPoint.ID, moves[Random.Next(0, moves.Length)].To.ID));

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}

				return true;
			}

			return false;
		}

		public static bool PlayBarToBoard(Simulator Simulator, Random Random, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			if (Player.BarCheckerCount == 0)
				return false;

			MoveInfo[] moves = Logic.GetPossibleBarToBoardMoves(board);

			if (moves == null || moves.Length == 0)
				return false;

			Simulator.SendEvent(new BarToBoardMoveEvent(board.TurnColor, moves[Random.Next(0, moves.Length)].To.ID));

			if (Serializer != null)
			{
				if (FullStep)
					Serializer.SerializeFullStep(Simulator.Frame);
				else
					Serializer.SerializeStep(Simulator.Frame);
			}

			return true;
		}

		public static bool PlayBearOff(Simulator Simulator, PlayerData Player, SessionSerializer Serializer = null, bool FullStep = false)
		{
			BoardData board = Simulator.Frame.Board;

			if (Utilities.GetInBaseCheckerCount(board.Points, board.TurnColor) + Player.BearedOffCheckersCount != ConfigData.PLAYER_CHECKER_COUNT)
				return false;

			for (int i = 0; i < ConfigData.POINT_COUNT && Player.MoveCount != 0; ++i)
			{
				PointData fromPoint = board.Points[i];

				MoveInfo[] moves = Logic.GetPossibleBearedOffs(board, fromPoint.ID);

				if (moves == null || moves.Length == 0)
					continue;

				Simulator.SendEvent(new BearOffEvent(fromPoint.ID));

				if (Serializer != null)
				{
					if (FullStep)
						Serializer.SerializeFullStep(Simulator.Frame);
					else
						Serializer.SerializeStep(Simulator.Frame);
				}

				return true;
			}

			return false;
		}
	}
}
