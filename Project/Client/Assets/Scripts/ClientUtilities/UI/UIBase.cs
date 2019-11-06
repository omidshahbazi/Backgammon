using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public delegate void UIShowed(GameObject UI);
    public delegate void UIHided(GameObject UI);
    public class UIBase : MonoBehaviour
    {
        public bool IsEnable
        {
            get { return this.gameObject.activeSelf; }
            set { this.gameObject.SetActive(value); }
        }

        [HideInInspector]
        public static event UIShowed OnUIShowed;
        [HideInInspector]
        public static event UIHided OnUIHided;
        [SerializeField]
        public AudioSource ShowSound = null;
        [SerializeField]
        public AudioSource CloseSound = null;
        protected bool IsRefrenceSet;
        public void RegisterUI(string Name, UIBase Item)
        {
            UIManager.Instance.AddUI(Name, Item);
        }

        public virtual void ShowUI(params object[] Args)
        {
            this.gameObject.SetActive(true);
            transform.SetAsLastSibling();
            OnUIShowed?.Invoke(this.gameObject);
            GameAnalyticsManager.Instance.SendUIOpened(this.gameObject.name);
        }

        public virtual void HideUI()
        {

            this.gameObject.SetActive(false);
            OnUIHided?.Invoke(this.gameObject);
            GameAnalyticsManager.Instance.SendUIClosed(this.gameObject.name);

        }

        public virtual void SetUIRefrences()
        {
            IsRefrenceSet = true;
        }

        protected virtual void Awake()
        {
            if (!IsRefrenceSet)
                SetUIRefrences();

        }

        protected virtual void Start()
        { }

        protected virtual void OnEnable()
        { }

        protected virtual void OnDisable()
        { }

        protected virtual void Update()
        { }

        protected virtual void LateUpdate()
        { }

        protected virtual void OnDestroy()
        { }
    }
}