
using BeardedManStudios.Forge.Networking;

namespace Networking.Common
{
	public static class Constants
	{
		public const string SERVER_ASCIIFY_TEXT = @"
//////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                          //
//    ____                   _						                    //
//   | __ )    __ _    ___  | | __   __ _    __ _   _ __ ___    _ __ ___     ___    _ __    //
//   |  _ \   / _` |  / __| | |/ /  / _` |  / _` | | '_ ` _ \  | '_ ` _ \   / _ \  | '_ \   //
//   | |_) | | (_| | | (__  |   <  | (_| | | (_| | | | | | | | | | | | | | | (_) | | | | |  //
//   |____/   \__,_|  \___| |_|\_\  \__, |  \__,_| |_| |_| |_| |_| |_| |_|  \___/  |_| |_|  //
//                                  |___/	                                            //
//                                                                                          //
//////////////////////////////////////////////////////////////////////////////////////////////
";

		public const int BINARY_FRAME_GROUP_ID = MessageGroupIds.START_OF_GENERIC_IDS + 1;

		public const int NULL_USER_ID = -1;

		public static readonly int[] LEADERBOARD_TYPE_HOURS = { 1, 24, 168, 99999 };

		public const string PACKAGE_NAME = "com.ZorvanGuys.RoyalGammon";
	}
}
