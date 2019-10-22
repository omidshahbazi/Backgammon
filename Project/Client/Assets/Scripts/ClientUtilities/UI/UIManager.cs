using Assets.Scripts.ClientUtilities.Pool;
using ClientUtilities.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class UIManager : MonoBehaviorSingleton<UIManager>
    {

        private Dictionary<string, UIBase> uiMap = new Dictionary<string, UIBase>();

        private void Awake()
        {
            UIBase[] list = FindObjectsOfType<UIBase>();
            if (list == null && list.Length == 0)
                return;
            for (ushort i = 0; i < list.Length; ++i)
            {
                UIBase ui = list[i];
                AddUI(ui.gameObject.name, ui);
            }
        }


        public void AddUI(string Name, UIBase Item)
        {
            if (uiMap.ContainsKey(Name))
                return;

            uiMap.Add(Name, Item);
        }

        public void ShowUI(string Name, params object[] Args)
        {
            Debug.Assert(uiMap.ContainsKey(Name), "This UI does not exist in the ui map please add it to the list");

            uiMap[Name].ShowUI(Args);
        }


    }
}