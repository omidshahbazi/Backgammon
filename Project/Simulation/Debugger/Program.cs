using Simulation.Common;
using Simulation.Data.Game;
using Simulation.Data.Mutation;
using Simulation.Data.Serialization;
using Simulation.Logic;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Simulation.Debugger
{
	class Program
	{
		static void Main(string[] args)
		{
			SessionDeserializer deserializer = new SessionDeserializer(File.ReadAllBytes("..\\..\\Client\\MemoryCard\\dump.bin"));

			ConfigData config = deserializer.DeserializeConfigDataState();
			FrameData frame = deserializer.DeserializeInitialState();
			Utilities.InitializeBoard(config, frame.Board);

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

				Utilities.PrintBoard(frame.Board);

				if (simulator.Frame.Hash != simulatedFrame.Hash)
				{
					DiffFinder.DiffInfoList diffs = new DiffFinder.DiffInfoList();
					DiffFinder.Find(frame.Board, simulatedFrame.Board, diffs);

					Debug.Assert(false);
				}
			}
		}
	}
}
