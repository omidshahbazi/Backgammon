using Networking.Common;
using GameFramework.ASCIISerializer;
using System;

namespace Networking.Server
{
	static class BotPlayerInfoMaker
	{
		//https://www.fantasynamegenerators.com/persian-names.php
		private static readonly string[] NAMES_PART_1 = {
				"Milad","Meysam","Behzad","Sadeq","Heshmat","Kaveh","Afshin","Mansour","Qobad","Faramarz","Mehrdad","Mojtaba","Moein","Pouya","Kooroush","Aref ","Vahid","Arsalan","Keyhan","Amin ","Reza ","Rasoul","Hassan","Hassan","Karim","Sasan","Jamshid","Behrouz","Arman","Aref ",
				"Farrokh","Kamyar","Mansour","Hassan","Arzhang","Abbas","Dariush","Enayat","Shadmehr","Farid","Pouria","Jamshid","Sepand","Javad","Mojtaba","Habib","Khosrow","Houshang","Fariborz","Salar","Zakaria","Kamyar","Ramin","Amir Ali","Ahmad","Farid","Vahid","Vahid","Pouya",
				"Nozar","Mehdi","Arsalan","Kooroush","Mehrdad","Payam","Kooroush","Ebrahim","Davoud","Changiz","Aziz ","Afshin","Youssef","Jamshid","Mahdi","Amir Ali","Pejman","Jamshid","Amir Reza ","Mostafa","Parviz","Arman","Javad","Heydar","Nouzar","Ali","Ahmad","Borzou","Mehrdad",
				"Sam","Sina ","Enayat","Faramarz","Payam","Parviz","Bahram","Khosrow","Freydoun","Asghar","Sina ","Sattar","Dariush","Mansour","Karim","Farid","Mani ","Karim","Mazyar","Kaveh","Hesam","Akbar","Dariush","Mansour","Karim","Farid","Mani ","Karim","Mazyar","Kaveh","Hesam",
				"Akbar","Zakaria","Abdi ","Foroutan","Teymoori","Mousavi","Chavoshi","Vahdat","Shojaii","Keramati","Tajik","Boromand","Bayat","Lotfi","Azadeh","Riahi","Aghili","Yekta","Danesh","Shakibaii ","Bayat","Aghili","Raeisi","Froozan","Fallah","Keramati","Faghih","Karimi",
				"Najafi","Zareii","Zand ","Noori","Teymoori","Darvishi","Jafarnejad","Gankhaki","Jafarnejad","Adib ","Panahi","Jahangiri ","Rastkar","Faghih","Bayat","Momeni","Rastkar","Baghaii","Golchin","Layegh","Dara ","Malakooti ","Tabasi","Mashayekhi","Kamran","Farzin","Noori",
				"Radan","Nassirian ","Blourian","Shookohi","Asadi","Haghighi","Esfahani","Aslani","Zangane","Esfahani","Eshtiaq","Nasirian","Meskini","Radish","Jafarnejad","Khosravi","Kamran","Mousavi","Haghshenas","Rastkar","Ershadi","Adib ","Foroutan","Afshar","Rouhani","Yaghmaei",
				"Zakaria","Arabnia","Deljou","Eskandari ","Barbarz","Qaderi","Shajarian ","Lorestani ","Yekta","Haghshenas","Sayyad","Golshani","Momeni","Karimi","Behdad","Gankhaki","Froozan","Sobhani","Dirbaz","Hashempour","Kardan","Almasi","Sehat","Asayesh","Hematti","Safavi",
				"Mehrian","Ershadi","Ershadi","Taghipour ","Kardan","Almasi","Sehat","Asayesh","Hematti","Safavi","Mehrian","Ershadi","Ershadi","Taghipour" };
		private static readonly string[] NAMES_PART_2 = {
				"khafan", "007", "king", "best", "man", "ghool", "GHUL", "Adineh", "Aria", "Ash", "Ayati", "Tehrani", "joojoo", "Joojoo", "Rizeh", "No", "the", "The", "fx", "shah", "ShaH", "mn", "HD", "vh", "sd", "PQ", "F.R", "Go", ">>>", "...", "..", ".", "   ", ",", "!", "!!",
				"?", "??", "***", "&", "&&", "$", "@", "test", "Test", "testi", "siah", "Naboodgar", "z", "war", "secret", "iran", "ir", "Iran", "io", "Ali", "kurd", "mah", "divune", "SUAREZ", "Mike", "Roney", "Silver", "gold", "me", "ME", "com", ".COM", "_", "__", "___", "0o0o",
				"killer", "king", "KING", "dragon", "Baby", "Teh", "021", "deth", "Killer", "KILLER", "killer", "TNT", "t.n.t", "Joker", "Batman", "Meee", "Death", "YOU", "Love", "Dead", "Dude", "Body", "MRS", "miss", "110", "pepe", "fury", "rage", "iOS", "iPhone", "Karaj",
				"karaji", "xxx", "xXx", "sss", "knight", "Night", "My", "devil", "Evil", "ooo", "Error", "Hacker", "hack", "Shakh", "SHAAKH", "Reza", "Family", "fateh", "aaa", "hero", "Hero", "GHOST", "ghost", "lol", "LOL", "LOOOL", ".sytem", ".H", ".M", ".G", ".B", "AAA", "Wolf",
				"Gorg", "gorgin", "saw", "SAW", "JIIIGH", "visitor", "DOTA", "amol", "..TV", "tv", "Watned", "WANTED", "FZW", "Gilaas", "*", "**", "***", "****", "parkour", "soccer", "KASPER", "www", "Tank", "tank baz", "aaa", "i.i.i", "!i!i!i", "Terminator", "takavar", "avatar",
				"TT", "khatar" };

		public static string Make(int SameUserID)
		{
			ISerializeObject obj = DatabaseLayer.GetUserInfo(SameUserID);

			string username = NAMES_PART_1[Configs.Random.Next(0, NAMES_PART_1.Length)];
			if (Configs.Random.Next(0, 101) % 2 == 0)
				username += " " + NAMES_PART_2[Configs.Random.Next(0, NAMES_PART_2.Length)];

			int coin = obj.Get<int>("coin");
			coin = Math.Max(0, Configs.Random.Next(coin - 1000, coin + 1000));

			int level = obj.Get<int>("coin");
			level = Math.Max(1, Configs.Random.Next(level - 2, level + 2));

			obj.Set("id", Constants.NULL_PLAYER_ID);
			obj.Set("username", username);
			obj.Set("coin", coin);
			obj.Set("level", level);

			return obj.Content;
		}
	}
}