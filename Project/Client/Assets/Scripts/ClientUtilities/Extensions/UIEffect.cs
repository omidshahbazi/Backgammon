using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using ClientUtilities.Singleton;
using System;
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using Assets.Scripts.GamePlayLogic.UI.UIItems;
using Assets.Scripts.ClientUtilities.Pool;
using ClientUtilities.ResourceManager;
using Assets.Scripts.GamePlayLogic.UI.ItemPool;

namespace Assets.Scripts.ClientUtilities.Extensions
{


    public class UIEffect : MonoBehaviorSingleton<UIEffect>
    {




        //public class XPNotificationItemFactory : IPoolObjectFactory<XPNotificationItem>
        //{
        //    public void AfterSendToPool(XPNotificationItem Object)
        //    { }

        //    public void BeforeGetFromPool(XPNotificationItem Object, object UserData = null)
        //    { }

        //    public void Destroy(XPNotificationItem Object)
        //    {
        //        GameObject.Destroy(Object.gameObject);
        //    }

        //    public XPNotificationItem Instantiate(object UserData = null)
        //    {
        //        return AssetsManager.Instance.CreateFromPrefab<XPNotificationItem>("Items/XPNotificationItem");
        //    }
        //}




        private struct TextNotificationData
        {
            public string Text;
            public Color Color;
        }

        private struct NotificationData
        {
            public int Amount;
            public string Name;
            public Sprite Sprite;
            public Vector3 StartPosition;
            public Transform EndPosition;
            public SpaceType StartSpace;
            public SpaceType EndSpace;
            public Action OnComplete;
            public bool NeedTrail;
        }

        public enum SpaceType
        {
            TwoD,
            ThreeD
        }

        public Sprite CoinSprite
        {
            get
            {
                return GameResourceManager.Instance.LoadSprite("Fantasy UI/Fantasy UI Sliced/Coin");
            }
        }

        private float tweenObjectTime = 1.5F;
        private float textTweenTime = 0.75F;
        private Camera cameraInstance;

        private RectTransform canvasRect;
        private List<NotificationData> notifications = null;
        private List<TextNotificationData> textNotificationList = null;
        private CollectedNotificationItemPool collecetedList = null;

        private Transform canvas2DTransform;
        private Transform canvas3DTransform;
        private bool isReadyForNextBuffNotification = true;

        private void Awake()
        {
            notifications = new List<NotificationData>();
            textNotificationList = new List<TextNotificationData>();
            collecetedList = new CollectedNotificationItemPool();

            //ObjectPool<XPNotificationItem>.Instance.Factory = new XPNotificationItemFactory();
            //ObjectPool<XPNotificationText>.Instance.Factory = new XPNotificationTextFactory();
            // ObjectPool<CollectedNotificationItem>.Instance.Factory = new CollectedNotificationItemFactory();
            collecetedList.InitiliazePool("UI/UIItems/CollectedNotificationItem", 10);

            canvas2DTransform = FindObjectOfType<Canvas>().transform;
            canvas2DTransform = FindObjectOfType<Canvas>().transform;


            cameraInstance = Camera.main;
            canvasRect = canvas2DTransform.GetComponent<RectTransform>();
        }

        private void Update()
        {


            if (textNotificationList != null && textNotificationList.Count != 0)
            {
                ShowTextNotification(textNotificationList[0]);
                textNotificationList.RemoveAt(0);
            }


            if (notifications == null || notifications.Count == 0)
                return;

            if (!LeanTween.isTweening(0/*itemLTToMiddleID*/))
            {
                NotificationData data = notifications[0];
                ShowAddNotification(data.Amount, data.Name, data.Sprite, data.StartPosition, data.EndPosition, data.StartSpace, data.EndSpace, data.OnComplete, data.NeedTrail);
                notifications.RemoveAt(0);
            }
        }

        public void AddNotification(bool NeedTrail, int Amount, string Name, Sprite Icon, Vector3 StartPosition, Transform EndPosition, SpaceType StartSpace, SpaceType EndSpace)
        {
            notifications.Add(new NotificationData { NeedTrail = NeedTrail, Amount = Amount, Name = Name, Sprite = Icon, StartPosition = StartPosition, EndPosition = EndPosition, StartSpace = StartSpace, EndSpace = EndSpace });
        }

        public void AddTextNotification(string NotificationText)
        {
            textNotificationList.Add(new TextNotificationData { Text = NotificationText, Color = Color.white });
        }

        public void AddTextNotification(string NotificationText, Color Color)
        {
            textNotificationList.Add(new TextNotificationData { Text = NotificationText, Color = Color });
        }


        private void ShowAddNotification(int Amount, string Name, Sprite ItemIcon, Vector3 StartPosition, Transform EndPosition, SpaceType StartSpace, SpaceType EndSpace, Action OnComplete = null, bool NeedTrail = false)
        {
            if (!NeedTrail)
            {

                CollectedNotificationItem notificationItem = collecetedList.GetFromPool();

                if (Amount != 0)
                    notificationItem.SetData(ItemIcon, Amount.ToString());
                else
                    notificationItem.SetData(ItemIcon, string.Empty);


                notificationItem.SetSize(new Vector2(0.45F, 0.45F), new Vector2(0.55F, 0.55F));

                notificationItem.transform.SetParent(canvas2DTransform, false);

                notificationItem.transform.SetAsLastSibling();
                notificationItem.AnchoredPosition = GetScreenPosition(StartPosition);

                notificationItem.transform.localScale = new Vector3(0.5F, 0.5F, 0.5F);

                LeanTween.scale(notificationItem.gameObject, Vector3.one, tweenObjectTime / 2);

                Vector2 endPos = GetScreenPosition(EndPosition.position);

                LeanTween.value(notificationItem.transform.gameObject, notificationItem.AnchoredPosition.y, endPos.y, tweenObjectTime / 2).setEase(LeanTweenType.easeInQuad).setDelay(tweenObjectTime / 2.0F).setOnComplete(() =>
                {
                    OnCompleteItemTween(notificationItem);
                    if (OnComplete != null)
                        OnComplete();
                }).setOnUpdate((value) =>
                {
                    notificationItem.AnchoredPosition = new Vector2(notificationItem.AnchoredPosition.x, value);
                });

                LeanTween.value(notificationItem.transform.gameObject, notificationItem.AnchoredPosition.x, endPos.x, tweenObjectTime / 2).setDelay(tweenObjectTime / 2.0F).setOnUpdate((value) =>
                {
                    notificationItem.AnchoredPosition = new Vector2(value, notificationItem.AnchoredPosition.y);
                });
            }

            else
            {
                int counter = 0;
                if (Amount != 0)
                    counter = Mathf.Min(5, Amount);
                else
                    counter = 7;

                bool isFirstTime = true;
                float delay = 0.0F;
                for (int i = 0; i < counter; ++i)
                {
                    delay += 0.1F;

                    ScheduleManager.Instance.AddSchedule(() =>
                        {
                            CollectedNotificationItem notificationItem = collecetedList.GetFromPool();

                            if (isFirstTime)
                            {
                                if (Amount != 0)
                                    notificationItem.SetData(ItemIcon, "+" + Amount);
                                else
                                    notificationItem.SetData(ItemIcon, string.Empty);

                                isFirstTime = false;
                            }
                            else
                                notificationItem.SetData(ItemIcon, string.Empty);


                            notificationItem.SetSize(new Vector2(0.45F, 0.45F), new Vector2(0.55F, 0.55F));

                            notificationItem.transform.SetParent(canvas2DTransform, false);

                            notificationItem.transform.SetAsLastSibling();
                            Vector3 startpos = GetScreenPosition(StartPosition);
                            Vector3 randomPos = new Vector3(UnityEngine.Random.Range(startpos.x - 20, startpos.x + 20), UnityEngine.Random.Range(startpos.y -20, startpos.y + 20));
                            notificationItem.AnchoredPosition = randomPos;

                            notificationItem.transform.localScale = new Vector3(0.5F, 0.5F, 0.5F);

                            LeanTween.scale(notificationItem.gameObject, Vector3.one, tweenObjectTime / 4);

                            Vector2 endPos = GetScreenPosition(EndPosition.position);

                            LeanTween.value(notificationItem.transform.gameObject, notificationItem.AnchoredPosition.y, endPos.y, tweenObjectTime / 2).setEase(LeanTweenType.easeInQuad).setDelay(tweenObjectTime / 3.0F).setOnComplete(() =>
                            {
                                OnCompleteItemTween(notificationItem);
                                if (OnComplete != null)
                                    OnComplete();
                            }).setOnUpdate((value) =>
                            {
                                notificationItem.AnchoredPosition = new Vector2(notificationItem.AnchoredPosition.x, value);
                            });

                            LeanTween.value(notificationItem.transform.gameObject, notificationItem.AnchoredPosition.x, endPos.x, tweenObjectTime / 2).setDelay(tweenObjectTime / 3.0F).setOnUpdate((value) =>
                            {
                                notificationItem.AnchoredPosition = new Vector2(value, notificationItem.AnchoredPosition.y);
                            });

                        }, delay);

                }

            }

        }

        private void ShowTextNotification(TextNotificationData TextData)
        {
            // TextNotificationWindow.Instance.ShowTextNotification(TextData.Text, TextData.Color);

        }




        //private void OnCompleteTextTween(CollectedNotificationText NotificationText)
        //{
        //    LeanTween.cancel(NotificationText.gameObject);
        //    ObjectPool<CollectedNotificationText>.Instance.SendToPool(NotificationText);
        //}

        private void OnCompleteItemTween(CollectedNotificationItem NotificationItem)
        {
            LeanTween.cancel(NotificationItem.gameObject);
            NotificationItem.SetImageState(false);
            //NotificationItem.SetParticleEmition(0);
            ScheduleManager.Instance.AddSchedule(() =>
            {
                collecetedList.SendToPool(NotificationItem);

            }, 0.75F);
        }



        public Vector3 GetScreenPosition(Vector3 transform)
        {
            Vector3 pos;
            float width = canvasRect.sizeDelta.x;
            float height = canvasRect.sizeDelta.y;
            float x = Camera.main.WorldToScreenPoint(transform).x / Screen.width;
            float y = Camera.main.WorldToScreenPoint(transform).y / Screen.height;
            pos = new Vector3(width * x - width / 2, y * height - height / 2);
            return pos;
        }

        private void OnUpdateToEnd(float Value, object Parameter)
        {
            object[] objs = Parameter as object[];

            Transform trans = (Transform)objs[0];
            LTDescr ltDescr = (LTDescr)objs[1];

            ltDescr.setTo(GetScreenPosition(trans.position));
        }



        private void OnToMiddleUpdate(float Value, object Parameter)
        {
            object[] objs = Parameter as object[];

            SpaceType type = (SpaceType)objs[0];
            Vector3 starsPos = (Vector3)objs[1];
            LTDescr ltDescr = (LTDescr)objs[2];

            ltDescr.setTo(GetCenter(type, starsPos));

        }



        private Vector3 GetCenter(SpaceType StartSpace, Vector3 StartPosition)
        {
            if (StartSpace == SpaceType.ThreeD)
            {
                float z = (StartPosition.z - cameraInstance.nearClipPlane) / 1;
                return cameraInstance.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, StartPosition.z * 3.0F));
            }
            else
            {
                return cameraInstance.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, canvas2DTransform.GetComponent<Canvas>().planeDistance));
            }
        }

        private Vector3[] GetArcPoints(Vector3 StartPosition, Vector3 EndPosition, SpaceType StartSpace)
        {
            Vector3[] Res = new Vector3[40];

            float radius = StartSpace == SpaceType.ThreeD ? 10.1F : 0.01F;

            float theta = 270 / (Res.Length);

            //Res[0] = StartPosition;
            Res[Res.Length - 1] = EndPosition;

            for (int i = 0; i < Res.Length - 1; ++i)
            {
                Vector3 position;
                float angle = (180 - (i * theta)) * Mathf.Deg2Rad;
                position.x = radius * Mathf.Cos(angle);
                position.y = radius * Mathf.Sin(angle);
                position.z = 0F;

                position += StartPosition;

                Res[i] = position;
            }

            return Res;
        }
    }
}