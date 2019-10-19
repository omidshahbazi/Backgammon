using Networking.Common;
using Simulation.Data.Event;
using Simulation.Data.Game;
using Simulation.Logic;
using GameFramework.BinarySerializer;
using GameFramework.ASCIISerializer;
using Networking.Server.Data;

namespace Networking.Server
{
	class OneByBotRoom : Room
	{
		private string botPlayerInfo;

		protected override Player WhitePlayer
		{
			get { return (PLayerCount == 1 ? Players[0] : null); }
		}

		protected override Player BlackPlayer
		{
			get { return null; }
		}

		public override string BotPlayerInfo
		{
			get { return botPlayerInfo; }
		}

		public OneByBotRoom(Application Application, uint TableEnterance, float TurnTime) :
			base(Application, TableEnterance, TurnTime)
		{
		}

		public override void Initialize()
		{
			ISerializeObject obj = BotPlayerInfoMaker.Make(WhitePlayer.ID);

			if (obj != null)
				botPlayerInfo = obj.Content;

			base.Initialize();
		}

		protected override void HandleSimulationEvent(int ClientHash, EventBase Event, Player Player, BufferStream Buffer)
		{
			SimulateEvent(Event);

			if (ClientHash != Simulator.Frame.Hash)
			{
				HandleGameFinisher(Player, GameFinishReasons.Mismatch);

				return;
			}

			if (Event.GetType() == EventBase.Types.FinishTurn)
			{
				PlayerData player = Utilities.GetPlayer(Simulator.Frame.Board, Simulator.Frame.Board.TurnColor);

				PlayAsBot(player);
			}
		}

		protected override int CreateGame()
		{
			return DatabaseLayer.CreateGame(DatabaseLayer.GameTypes.OneByBot, Enterance, WhitePlayer.Version);
		}

		protected override void HandleGetGameData(Player Player)
		{
			++ReadyPlayerCount;

			base.HandleGetGameData(Player);

			SendBuffer.Reset();
			SendBuffer.WriteBytes(Commands.Category.ROOM, Commands.Room.GET_GAME_DATA);

			SendBuffer.WriteInt32((int)PlayerColors.White);

			Send(Player, SendBuffer);
		}
	}
}