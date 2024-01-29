using BeardedManStudios.Forge.Networking;
using Networking.Common;
using GameFramework.BinarySerializer;
using Networking.Server.Data;

namespace Networking.Server
{
	class Admin : LogicObjects
	{
		private BufferStream sendBuffer = null;

		public Admin(Application Application) :
			base(Application)
		{
			sendBuffer = new BufferStream(new byte[Configs.NetworkConfig.SendBufferSize * 100]);
		}

		public void HandleRequest(BufferStream Buffer, NetworkingPlayer Player)
		{
			byte command = Buffer.ReadByte();

			if (command == Commands.Admin.GET_STATISTICS)
			{
				HandleGetStatus(Buffer, Player);
			}
		}

		private void HandleGetStatus(BufferStream Buffer, NetworkingPlayer Player)
		{
			sendBuffer.ResetWrite();
			sendBuffer.WriteBytes(Commands.Category.ADMIN, Commands.Admin.GET_STATISTICS);
			sendBuffer.WriteString(Application.GetStatistics().Content);

			Send(Player, sendBuffer);
		}
	}
}