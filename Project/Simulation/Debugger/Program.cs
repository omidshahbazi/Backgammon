using GameFramework.Common.Utilities;
using Simulation.Data.Game;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Simulation.Debugger
{
	class Program
	{
		private const string DEFAULT_PATH = "..\\..\\Client\\MemoryCard\\dump.bin";

		static void Main(string[] args)
		{
			string path = DEFAULT_PATH;

			while (true)
			{
				if (!ConsoleHelper.GetConfirmation("Would you like to proceed with [" + path + "] ?"))
					ConsoleHelper.ReadString("Enter dump path: ", out path);

				if (!File.Exists(path))
				{
					Console.WriteLine("File [" + path + "] doesn't exists");

					continue;
				}

				break;
			}

			SessionDeserializer deserializer = new SessionDeserializer(File.ReadAllBytes(path));

			ConfigData config = deserializer.DeserializeConfigDataState();
			FrameData frame = deserializer.DeserializeInitialState();
			InitializeUtilities.InitializeBoard(config, frame.Board);

			List<FrameData> frames = new List<FrameData>();

			FrameData stepFrame = null;
			while ((stepFrame = deserializer.DeserializeFullStep()) != null)
				frames.Add(stepFrame);

			Simulator simulator = new Simulator();
			simulator.SetConfig(config);
			simulator.SetFrame(frame);

			for (int i = 0; i < frames.Count; ++i)
			{
				FrameData simulatedFrame = frames[i];

				simulator.SendEvent(simulatedFrame.Events[0]);

				Console.WriteLine(simulator.Frame.Board.TurnNumber);
				Utilities.PrintBoard(frame.Board);

				if (simulator.Frame.Hash != simulatedFrame.Hash)
				{
					DiffFinder.DiffInfoList diffs = new DiffFinder.DiffInfoList();
					DiffFinder.Find(frame.Board, simulatedFrame.Board, diffs);

					Debug.Assert(false);
				}
			}

			Console.ReadLine();
		}
	}
}
