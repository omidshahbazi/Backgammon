using Simulation.Data.Game;
using Simulation.Data.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Simulation.Debugger
{
	class Program
	{
		static void Main(string[] args)
		{
			SessionDeserializer deserializer = new SessionDeserializer(File.ReadAllBytes("D:/dump.bin"));

			ConfigData config = deserializer.DeserializeConfigDataState();
			FrameData frame = deserializer.DeserializeInitialState();

			List<FrameData> frames = new List<FrameData>();

			FrameData stepFrame = null;
			while ((stepFrame = deserializer.DeserializeFullStep()) != null)
				frames.Add(stepFrame);


		}
	}
}
