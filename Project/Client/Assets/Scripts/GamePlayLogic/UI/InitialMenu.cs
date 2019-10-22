using Assets.Scripts.GamePlayLogic.RequestManagers;
using Networking.Common;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.ClientUtilities.Extensions;
using Assets.Scripts.ClientUtilities.Pool;
using Assets.Scripts.GamePlayLogic.Tables;
using UnityEngine;
using MagneticScrollView;
using System.Collections.Generic;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public class TablePool : ObjectPool<TableItem>
    { }

    public class InitialMenu : UIBase
    {
        private TablePool tableList = new TablePool();
        private List<TableItem> activeTableItem = new List<TableItem>();
        private RectTransform viewPortTransform;
        private MagneticScrollRect scrollView;


        protected override void Awake()
        {
            base.Awake();
            RegisterUI("InitialMenu", this);
        }



        public override void ShowUI(object[] Args)
        {
            base.ShowUI(Args);

            float width = (viewPortTransform.rect.width - (scrollView.ElementPadding * 2)) /1.5F;
            float height = viewPortTransform.rect.height / 1.4F;
            scrollView.ElementsSize = new Vector2(width, height);
            for (int i = 0; i < TablesManager.Instance.Tables.Length; ++i)
            {
                TableItem it = null;
                activeTableItem.Add(it = tableList.GetFromPull());
                TablesManager.Table table = TablesManager.Instance.Tables[i];
                it.transform.SetParent(viewPortTransform, false);
                it.transform.SetAsLastSibling();
                it.SetData(() => JoinTable(table.Enterance), table.Name, table.Enterance.ToString(), "");
            }
        }

        public void JoinTable(uint Enterance)
        {
            RequestManager.Instance.Network.JoinToRoom(Enterance, true);
            HideUI();
        }

        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();
            tableList.InitiliazePool("UI/UIItems/TableItem", 3);
            RegisterUI("InitialMenu", this);
            viewPortTransform = transform.FindDeep("Viewport").GetComponent<RectTransform>();
            scrollView = transform.FindDeep("Magnetic Scroll View").GetComponent<MagneticScrollRect>();
        }

    }
}