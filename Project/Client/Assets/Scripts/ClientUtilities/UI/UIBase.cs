using ClientUtilities.AudioMangaer;
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

        public string ShowAudioPath;
        public string CloseAudioPath;
        private string defaultPath = "ViewChange";
        private Audio ShowSound = null;
        private Audio CloseSound = null;
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
            if (ShowSound != null)
            {
                ShowSound.Stop();
                ShowSound.Play();
            }
            GameAnalyticsManager.Instance.SendUIOpened(this.gameObject.name);
        }

        public virtual void HideUI()
        {

            this.gameObject.SetActive(false);
            OnUIHided?.Invoke(this.gameObject);
            if (CloseSound != null)
            {
                CloseSound.Stop();
                CloseSound.Play();
            }
            GameAnalyticsManager.Instance.SendUIClosed(this.gameObject.name);

        }

        public virtual void SetUIRefrences()
        {
            string defaultstring;
            IsRefrenceSet = true;
            if (ShowSound == null)
            {

                if (ShowAudioPath == string.Empty)
                    defaultstring = defaultPath;
                else
                    defaultstring = ShowAudioPath;
                ShowSound = AudioManager.Instance.Load(defaultstring, AudioManager.SoundTypes.Effect);
                ShowSound.AutoUnload = false;
                ShowSound.Volume = 100;
            }

            if (CloseSound == null)
            {
                if (CloseAudioPath == string.Empty)
                    defaultstring = defaultPath;
                else
                    defaultstring = CloseAudioPath;
                CloseSound = AudioManager.Instance.Load(defaultstring, AudioManager.SoundTypes.Effect);
                CloseSound.AutoUnload = false;
                CloseSound.Volume = 100;
            }
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