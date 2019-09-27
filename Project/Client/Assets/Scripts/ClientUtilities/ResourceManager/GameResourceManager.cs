
using ClientUtilities.Singleton;
using System.Collections;
using UnityEngine;

namespace ClientUtilities.ResourceManager
{
	public class GameResourceManager : MonoBehaviorSingleton<GameResourceManager>
	{
		public uint CountOfAssetLoaded
		{
			get;
			private set;
		}

		private Hashtable assetsHashTable = new Hashtable();

		public Texture LoadTexture(string Path)
		{
			return (Texture)GetFormAssetHashTable("Textures/" + Path);
		}

		public Material LoadMaterial(string Path)
		{
			return (Material)GetFormAssetHashTable("Materials/" + Path);
		}

		public GameObject LoadPrefab(string Path)
		{
			return (GameObject)GetFormAssetHashTable("Prefabs/" + Path);
		}

		public Font LoadFont(string Path)
		{
			return (Font)GetFormAssetHashTable("Fonts/" + Path);
		}

		public AudioClip LoadAudioClip(string Path)
		{
			return (AudioClip)GetFormAssetHashTable("Audios/" + Path);
		}

		public Sprite LoadSprite(string Path)
		{
			return (Sprite)GetFormAssetHashTable("Sprites/" + Path);
		}

        public void UnloadPrefab(string Path)
		{
			UnloadAsset("Prefabs/" + Path);
		}

		public void UnloadMaterial(string Path)
		{
			UnloadAsset("Materials/" + Path);
		}

		public void UnloadTexture(string Path)
		{
			UnloadAsset("Textures/" + Path);
		}

		public void UnloadFont(string Path)
		{
			UnloadAsset("Fonts/" + Path);
		}

		public void UnloadAudioClip(string Path)
		{
			UnloadAsset("Audios/" + Path);
		}

		public void UnloadFontSprite(string Path)
		{
			UnloadAsset("Sprites/" + Path);
		}

		public bool UnloadAllAssets()
		{

			var iterator = assetsHashTable.GetEnumerator();

			while (iterator.MoveNext())
			{
				Object current = (Object)iterator.Value as Object;

				if (current.GetType() != typeof(GameObject))
					Resources.UnloadAsset(current);

				if (CountOfAssetLoaded > 0)
					--CountOfAssetLoaded;
				current = null;
			}
			assetsHashTable.Clear();
			Resources.UnloadUnusedAssets();
			return CountOfAssetLoaded == 0 ? true : false;
		}

		private bool UnloadAsset(string Path)
		{
			bool isUnloaded = false;
			int hashCode = Path.GetHashCode();
			if (assetsHashTable.ContainsKey(hashCode))
			{
				Object unloadObject = (Object)assetsHashTable[hashCode] as Object;
				if (unloadObject.GetType() != typeof(GameObject))
					Resources.UnloadAsset(unloadObject);
				assetsHashTable.Remove(hashCode);
				isUnloaded = true;
				if (CountOfAssetLoaded > 0)
					--CountOfAssetLoaded;
			}
			return isUnloaded;
		}

		private Object GetFormAssetHashTable(string Path)
		{
			Object loadedObject = new Object();

			int hashCode = Path.GetHashCode();
			if (assetsHashTable.ContainsKey(hashCode))
				loadedObject = (Object)assetsHashTable[hashCode] as Object;
			else
				assetsHashTable.Add(hashCode, loadedObject);

			if (loadedObject == null)
			{
				loadedObject = Resources.Load(Path, loadedObject.GetType());

				if (loadedObject == null)
				{
					assetsHashTable.Remove(hashCode);
					return null;
				}
				assetsHashTable[hashCode] = loadedObject;
			}

			++CountOfAssetLoaded;
			return loadedObject;
		}
	}
}