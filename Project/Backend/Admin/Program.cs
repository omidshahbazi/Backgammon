using System.Threading;

namespace Networking.Admin
{
	class Program
	{
		static void Main(string[] args)
		{
			Application application = new Application();

			while (true)
			{
				Thread.Sleep(1000);

				application.Update();
			}
		}
	}
}