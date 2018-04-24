using Photon.SocketServer;

namespace GameServer.Server
{
	class Application : ApplicationBase
	{
		protected override PeerBase CreatePeer(InitRequest InitRequest)
		{
			return new ClientInstance(InitRequest);
		}

		protected override void Setup()
		{
		}

		protected override void TearDown()
		{
		}
	}
}