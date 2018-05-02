// Copyright 2015-2016 Zorvan Game Studio. All Rights Reserved.
using GameServer.Common;
using System;

namespace GameServer.Server
{
	class GameInstance
	{
		private Random random = new Random();

		public ClientInstance FirstClient
		{
			get;
			private set;
		}

		public ClientInstance SecondClient
		{
			get;
			private set;
		}

		public GameInstance(ClientInstance FirstClient)
		{
			this.FirstClient = FirstClient;
		}

		public void Join(ClientInstance Client)
		{
			SecondClient = Client;

			int dice1, dice2;
			GetDice(out dice1, out dice2, true);

			FirstClient.SendOperation(MessageTypes.MatchFound, ParameterTypes.UDID, SecondClient.UDID, ParameterTypes.Dice1, dice1, ParameterTypes.Dice2, dice2);
			SecondClient.SendOperation(MessageTypes.MatchFound, ParameterTypes.UDID, FirstClient.UDID, ParameterTypes.Dice1, dice2, ParameterTypes.Dice2, dice1);

			GameManager.Instance.MakeOnGoing(this);
		}

		public void GetDice(out int Dice1, out int Dice2, bool ExcludeSame)
		{
			Dice1 = GetSingleDice();
			Dice2 = GetSingleDice();

			if (!ExcludeSame)
			{
				if (Dice1 != Dice2)
					return;

				if (Dice1 == 1)
					++Dice1;
				else
					--Dice1;
			}
		}

		private int GetSingleDice()
		{
			return random.Next(1, 7);
		}
	}
}