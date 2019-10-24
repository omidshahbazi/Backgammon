using ClientUtilities.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ClientUtilities.ScheduleSystem
{


    public class ScheduleObj
    {
        public bool IsDone
        {
            get
            {
                return OnComplete == null;
            }
        }

        private Action OnComplete;  
        private float delay = 0.0F;
        private float deliverTime = 0.0F;

        public ScheduleObj(Action onComplete, float delay = 0.0F)
        {
            OnComplete = onComplete;
            this.delay = delay;
            this.deliverTime = Time.time + delay;
        }

        public void CancelSchedule()
        {
            OnComplete = null;
        }

        public void Update()
        {
            if (Time.time < deliverTime)
                return;

            OnComplete?.Invoke();
            OnComplete = null;
        }

       
    }




    public class ScheduleManager : MonoBehaviorSingleton<ScheduleManager>
    {

        private List<ScheduleObj>  scheduleList  = new List<ScheduleObj>();

        public ScheduleObj AddSchedule(Action Action , float Delay =0.0f)
        {
            ScheduleObj obj = new ScheduleObj(Action, Delay);
            scheduleList.Add(obj);
            return obj;
        }

        private void Update()
        {
            for(int i =0; i<scheduleList.Count;++i)
                scheduleList[i].Update();
          

            for(int i =0;i<scheduleList.Count;++i)
            {
                if (!scheduleList[i].IsDone)
                    continue;

                scheduleList.RemoveAt(i--);
            }

        }
    }
}
