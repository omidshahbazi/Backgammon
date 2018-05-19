using System;
using System.Collections.Generic;

namespace GameServer.Server
{
	class GameManager
	{
		private static GameManager instance = null;

		public static GameManager Instance
		{
			get
			{
				if (instance == null)
					instance = new GameManager();

				return instance;
			}
		}

		private List<GameInstance> games = new List<GameInstance>();
		private List<GameInstance> waitingGames = new List<GameInstance>();

		public void AddWaitingGame(GameInstance Game)
		{
			if (waitingGames.Contains(Game))
				return;

			for (int i = 0; i < waitingGames.Count; ++i)
				if (waitingGames[i].FirstClient == Game.FirstClient)
					return;

			waitingGames.Add(Game);
		}

		public void RemoveWaitingGame(GameInstance Game)
		{
			waitingGames.Remove(Game);
		}

		public void MakeOnGoing(GameInstance Game)
		{
			waitingGames.Remove(Game);
			games.Add(Game);
		}

		public void HandleClientDisconnected(ClientInstance Client)
		{
			GameInstance game = GetWaitingByClient(Client);
			if (game != null)
				waitingGames.Remove(game);

			game = GetByClient(Client);
			if (game != null)
				games.Remove(game);
		}

		public GameInstance GetWaitingByClient(ClientInstance Client)
		{
			HandleDisposedClients();

			for (int i = 0; i < waitingGames.Count; ++i)
				if (waitingGames[i].FirstClient == Client)
					return waitingGames[i];

			return null;
		}

		public GameInstance GetByClient(ClientInstance Client)
		{
			HandleDisposedClients();

			for (int i = 0; i < games.Count; ++i)
			{
				GameInstance game = games[i];

				if (game.FirstClient == Client || game.SecondClient == Client)
					return game;
			}

			return null;
		}

		private void HandleDisposedClients()
		{
			for (int i = 0; i < waitingGames.Count; ++i)
				if (waitingGames[i].FirstClient.ConnectionState != Photon.SocketServer.ConnectionState.Connected)
					waitingGames.RemoveAt(i--);
		}
	}
}