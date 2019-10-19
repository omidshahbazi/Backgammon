using ClientUtilities.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ClientUtilities.ScheduleSystem
{
    public class ScheduleManager : MonoBehaviorSingleton<ScheduleManager>
    {
        public void ScheduleAction(Action Action,float Time = 0)
        {
            StartCoroutine(DoScheudle(Action, Time));
        }

        private IEnumerator DoScheudle(Action Action, float Time = 0)
        {
            yield return new WaitForSeconds(Time);

            Action?.Invoke();
        }
    }
}
