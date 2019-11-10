using System;
using UnityEngine;
using System.Collections.Generic;
using GameFramework.ASCIISerializer;

namespace Assets.Scripts.ClientUtilities.Extensions
{
    public static class JsonExtensions
    {
        public static bool IsContains(this ISerializeObject obj, string Key)
        {
            if (obj.Contains(Key))
                return true;
            else
            {
                GameAnalyticsManager.Instance.SendErrorEvent(GameAnalyticsSDK.GAErrorSeverity.Warning, Key + "[key Does not exist]");
                Debug.LogWarning(Key + "[key Does not exist]");
                return false;
            }
        }
    }
}