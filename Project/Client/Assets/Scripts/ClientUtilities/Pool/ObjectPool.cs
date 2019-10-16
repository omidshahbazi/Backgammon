using ClientUtilities.ResourceManager;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ClientUtilities.Pool
{
    public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        public int Count
        {
            get { return Pool.Count; }
        }

        private string TemplatePrefabPath;
        private Stack<T> Pool = null;

        public void InitiliazePool(string Path, int Count = 0)
        {
            Debug.Assert(TemplatePrefabPath != string.Empty, "Path is Empty");
            TemplatePrefabPath = Path;
            Pool = new Stack<T>(Count);

            for (int i = 0; i < Count; ++i)
                SendToPool(Instantiate(GameResourceManager.Instance.LoadPrefab(Path),
                                       Vector3.zero, Quaternion.identity).GetComponent<T>());
        }

        public void SendToPool(T Item)
        {
            Debug.Assert(!Contains(Item), "Item exist in the pool");
            if (Item == null)
                return;

            Item.gameObject.SetActive(false);
            Pool.Push(Item);
        }

        public T GetFromPull()
        {
            Debug.Assert(TemplatePrefabPath != string.Empty, "First of all intilize the pool");
            if (Pool.Count == 0)
                SendToPool(Instantiate(GameResourceManager.Instance.LoadPrefab(TemplatePrefabPath),
                                     Vector3.zero, Quaternion.identity).GetComponent<T>());
            Pool.Peek().gameObject.SetActive(true);
            return Pool.Pop();
        }

        public bool Contains(T Item)
        {
            if (Item == null)
                return false;

            return Pool.Contains(Item);
        }

        public void Clear()
        {
            Pool.Clear();
        }

        public T GetItemTypeOfPoolObject()
        {
            if (Pool.Count == 0)
                return null;

            return Pool.Peek();
        }
    }
}
