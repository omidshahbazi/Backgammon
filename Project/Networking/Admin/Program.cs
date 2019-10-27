using Networking.Common;
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
				Thread.Sleep(1);

				application.Update();
			}
		}
	}
}