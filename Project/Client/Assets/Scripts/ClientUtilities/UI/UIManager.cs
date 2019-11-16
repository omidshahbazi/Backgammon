using Assets.Scripts.ClientUtilities.Pool;
using ClientUtilities.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.ClientUtilities.ScheduleSystem;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class UIManager : MonoBehaviorSingleton<UIManager>
    {
        public LeaderBoarItemPool leaderBoolItemPool = new LeaderBoarItemPool();
        private Dictionary<string, UIBase> uiMap = new Dictionary<string, UIBase>();


        private void Awake()
        {
            List<UIBase> list = new List<UIBase>();
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; ++i)
            {
                list.AddRange(roots[i].GetComponentsInChildren<UIBase>(true));
                for (int j = 0; j < list.Count; ++j)
                {
                    UIBase ui = list[j];
                    AddUI(ui.gameObject.name, ui);
                   // ScheduleManager.Instance.AddThreadedSchedule(()=>SetSpeceficUIRefrences(ui.gameObject.name), null);
                    SetSpeceficUIRefrences(ui.gameObject.name);
                }
                list.Clear();
            }
            ScheduleManager.Instance.AddThreadedSchedule(() =>
            {
                leaderBoolItemPool.InitiliazePool("UI/UIItems/LeaderBoardItem", 20);
            }, null);
          
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


        public void HideUI(string Name)
        {
            Debug.Assert(uiMap.ContainsKey(Name), "This UI does not exist in the ui map please add it to the list");

            uiMap[Name].HideUI();
        }

        public void SetSpeceficUIRefrences(string Name)
        {
            Debug.Assert(uiMap.ContainsKey(Name), "This UI does not exist in the ui map please add it to the list");
            if (uiMap[Name].IsEnable)
                uiMap[Name].SetUIRefrences();
            else
            {
                uiMap[Name].IsEnable = true;
                uiMap[Name].SetUIRefrences();
                uiMap[Name].IsEnable = false;
            }
        }

    }
}