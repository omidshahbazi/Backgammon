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

			SimulationLogic simulation = new SimulationLogic();
			HasherVisitor hasher = new HasherVisitor();

			for (int i = 0; i < frames.Count; ++i)
			{
				FrameData simulatedFrame = frames[i];

				MutationList mutations = new MutationList();
				simulation.Simulate(config, frame.Board, simulatedFrame.Events, mutations);

				hasher.Reset();
				frame.Board.Visit(hasher);

				if (hasher.Value != simulatedFrame.Hash)
				{
					DiffFinder.DiffInfoList diffs = new DiffFinder.DiffInfoList();
					DiffFinder.Find(frame.Board, simulatedFrame.Board, diffs);

					//Debug.Assert(false);
				}
			}
		}
	}
}
