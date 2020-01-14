using Assets.Scripts.ClientUtilities.Extensions;
using ClientUtilities.ResourceManager;
using Networking.Common;
using Simulation.Data.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{

    public class BarOff : MonoBehaviour
    {
        public enum Side
        {
            Down,
            UP,
            Left,
            Reight,
        }

        [SerializeField]
        public int ID = 0;
        [SerializeField]
        public Side BarSide;
        [SerializeField]
        public PlayerColors Color;
        [SerializeField]
        public float BeedStartPosition;
        [SerializeField]
        public Vector2 PointBond;
        [SerializeField]
        public Vector2 CheckerBond;


        public int BarCheckerCount
        {
            get;
            set;
        }


        public List<Beed> pointBeeds
        {
            get;
            set;
        }


        public bool SetHighlightHelper
        {
            get
            {
                return HighlightHeleper.gameObject.activeSelf;
            }

            set
            {
                HighlightHeleper.gameObject.SetActive(value);

            }
        }


        private GameObject HighlightHeleper;
        private SpriteRenderer sprite;
        private static GameObject WhiteBeed = null;
        public static GameObject BlackBeed = null;


        private void Awake()
        {
            pointBeeds = new List<Beed>();
            WhiteBeed = GameResourceManager.Instance.LoadPrefab("WhiteBead");
            BlackBeed = GameResourceManager.Instance.LoadPrefab("BlackBead");
            sprite = WhiteBeed.GetComponent<SpriteRenderer>();
            HighlightHeleper = transform.FindDeep("HighlightUp", true).gameObject;

#if !BACKGAMOON_NEW_GAME_PLAY_VERSION
            HighlightHeleper.GetComponent<SpriteRenderer>().enabled = false;
            HighlightHeleper.gameObject.SetActive(true);
#endif
            PointVisualizerManager.Instance.OnUpdatePointsData += OnUpdatePointsData;
          //  SimulationManager.Instance.OnGameFinished += Instance_OnGameFinished;

        }

        //private void Instance_OnGameFinished(PlayerColors WinnerColor, GameFinishReasons Reason, int Score)
        //{
        //    //SendToPool();
        //}

        public void Rearrange()
        {
            if (pointBeeds.Count == 0)
                return;

            Vector2[] positions = FindPositions();
            float zOffset = -0.15F;
            for (int i = BarCheckerCount - 1; i > -1; --i)
            {
                if (i >= pointBeeds.Count)
                    continue;

                GameObject go = pointBeeds[i].gameObject;
                go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, 0);
                go.transform.position = positions[i];
                Vector2 pos = positions[i];
                go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, zOffset);
                //LeanTween.move(go, pos, 0.1F).setEase(LeanTweenType.linear);

                zOffset += 0.01F;
            }

        }


        public Vector2[] FindPositions()
        {
            if (BarCheckerCount == 0)
                return null;

            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < BarCheckerCount; ++i)
                list.Add(FindPosition(i));

            return list.ToArray();
        }

        //Always you have should send count+1 if you want find empty space
        public Vector2 FindPosition(int Count)
        {
            float percent = 0;
            if (pointBeeds.Count < 7)
                percent = 2.5F;
            else
                percent = 1.5F;
            float space = pointBeeds.Count <= 5 ? 0 : (sprite.sprite.bounds.size.x / percent);

            if (BarSide == Side.UP || BarSide == Side.Down)
            {
                float offset = sprite.sprite.bounds.size.x;
                float yPosition = BarSide == Side.UP ? BeedStartPosition - ((sprite.sprite.bounds.size.x - space) * (Count))
                    : BeedStartPosition + ((sprite.sprite.bounds.size.x - space) * (Count));
                return new Vector2(this.transform.position.x, yPosition);

            }
            else
            {
                float offset = sprite.sprite.bounds.size.x;
                float xPosition = BarSide == Side.Reight ? BeedStartPosition - ((sprite.sprite.bounds.size.x - space) * (Count))
                    : BeedStartPosition + ((sprite.sprite.bounds.size.x - space) * (Count));
                return new Vector2(xPosition, transform.position.y);

            }
        }

        private void OnUpdatePointsData()
        {


            for (int i = 0; i < BarCheckerCount; ++i)
            {
                Beed tempBeed = null;
                if (Color == PlayerColors.White)
                {
                    tempBeed = TableManager.Instance.WhiteBeads.GetFromPool();
                }
                else
                {
                    tempBeed = TableManager.Instance.BlackBeads.GetFromPool();

                }

                pointBeeds.Add(tempBeed);
                tempBeed.transform.SetParent(this.transform);
                tempBeed.transform.position = FindPosition(i);
            }

            Rearrange();
        }

        public void SendToPool()
        {
            for (int i = 0; i < pointBeeds.Count; ++i)
            {
                Beed b = pointBeeds[i];
                if (b.BeedColor == PlayerColors.White)
                    TableManager.Instance.WhiteBeads.SendToPool(b);
                else
                    TableManager.Instance.BlackBeads.SendToPool(b);

                pointBeeds.Remove(b);
                i--;
            }


            //if (pointBeeds.Count != 0)
            //{
            //    for (int i = 0; i < pointBeeds.Count; ++i)
            //    {
            //        if (pointBeeds.Peek().BeedColor == PlayerColors.White)
            //            TableManager.Instance.WhiteBeads.SendToPool(pointBeeds.Pop());
            //        else
            //            TableManager.Instance.BlackBeads.SendToPool(pointBeeds.Pop());

            //        --i;
            //    }

            //}
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (WhiteBeed == null)
            {
                WhiteBeed = GameResourceManager.Instance.LoadPrefab("WhiteBead");
            }

            if (WhiteBeed != null && sprite == null)
                sprite = WhiteBeed.GetComponent<SpriteRenderer>();

            if (sprite == null)
                return;


            if (BarSide == Side.UP || BarSide == Side.Down)
            {
                // Gizmos.DrawWireCube(this.transform.position, PointBond);
                Gizmos.DrawSphere(new Vector3(this.transform.position.x, BeedStartPosition, 0), sprite.sprite.bounds.extents.x);
            }
            else
                Gizmos.DrawSphere(new Vector3(BeedStartPosition, this.transform.position.y, 0), sprite.sprite.bounds.extents.x);

            for (int i = 0; i < 8; ++i)
            {
                if (BarSide == Side.UP || BarSide == Side.Down)
                {
                    float yOffset = BarSide == Side.UP ? (BeedStartPosition - (sprite.sprite.bounds.size.x * i)) : BeedStartPosition + ((sprite.sprite.bounds.size.x * i));
                    Gizmos.DrawWireSphere(new Vector3(this.transform.position.x, yOffset, 0), sprite.sprite.bounds.extents.x);
                }
                else
                {
                    float xOffset = BarSide == Side.Reight ? (BeedStartPosition - (sprite.sprite.bounds.size.x * i)) : BeedStartPosition + ((sprite.sprite.bounds.size.x * i));
                    Gizmos.DrawWireSphere(new Vector3(xOffset, transform.position.y, 0), sprite.sprite.bounds.extents.x);
                }
            }

        }
#endif
    }
}
