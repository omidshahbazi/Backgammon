using System;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.EventSystem
{
    public delegate void TableUpdateHandler(int TableID);

    public static partial class EventManager
    {
        public static event TableUpdateHandler OnTableDataUpdate;

        public static void OnTableDataUpdateCall(int TableID)
        {
            OnTableDataUpdate?.Invoke(TableID);
        }
    }
}
