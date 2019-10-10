
//using System.IO;
//using UnityEngine;

//namespace Assets.Scripts.ClientUtilities
//{
//	static class FileSystem
//	{
//		public static string Path
//		{
//			get { return Application.dataPath + "\\..\\MemoryCard\\"; }
//		}

//		static FileSystem()
//		{
//			if (!Directory.Exists(Path))
//				Directory.CreateDirectory(Path);
//		}

//		public static void WriteBytes(string Name, byte[] Data)
//		{
//			File.WriteAllBytes(Path + Name, Data);
//		}
//	}
//}