using ClientUtilities.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientUtilities.Tap
{
    public class Tap : MonoBehaviorSingleton<Tap>
    {
        public delegate void TapBegin(Vector2 Position);
        public event TapBegin OnTapBegin;
        private void Update()
        {

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                OnTapBegin?.Invoke(Input.mousePosition);
#else
            if (Input.touchCount == 0)
                return;
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                OnTapBegin?.Invoke(touch.position);
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
