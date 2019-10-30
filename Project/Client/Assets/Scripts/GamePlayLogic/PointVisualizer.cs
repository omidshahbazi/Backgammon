using ClientUtilities.ResourceManager;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
    public class PointVisualizer : MonoBehaviour
    {
        public enum Side
        {
            Down,
            UP
        }

        [SerializeField]
        public Side PointVisualizerSide;
        [SerializeField]
        public float BeedStartPositionY;
        [SerializeField]
        public Vector2 PointBond;
        [SerializeField]
        public Vector2 CheckerBond;


        [SerializeField]
        public GameObject HighlightHeleper;



        public PointData PointData
        {
            get;
            set;
        }


        public int Index
        {
            get;
            set;
        }

        public Stack<Beed> pointBeeds
        {
            get;
            set;
        }

        private static SpriteRenderer sprite;


        private void Awake()
        {
            pointBeeds = new Stack<Beed>();

            if (sprite == null)
                sprite = GameResourceManager.Instance.LoadPrefab("WhiteBead").GetComponent<SpriteRenderer>();
           
            PointVisualizerManager.Instance.OnUpdatePointsData += OnUpdatePointsData;

        }


        public Vector2[] FindPositions()
        {
            if (PointData == null || PointData.CheckerCount == 0)
                return null;

            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < PointData.CheckerCount; ++i)
                list.Add(FindPosition(i));

            return list.ToArray();
        }

        //Always you have should send count+1 if you want find empty space
        public Vector2 FindPosition(int Count)
        {

           float count = Count == 5 ? Count : (Count / 15f);
            float offset = sprite.sprite.bounds.size.x;
            float yPosition = PointVisualizerSide == Side.UP ? BeedStartPositionY - (sprite.sprite.bounds.size.x * (count))
                : BeedStartPositionY + (sprite.sprite.bounds.size.x * (count));
            return new Vector2(this.transform.position.x, yPosition);
        }

        private void OnUpdatePointsData()
        {

            //if (PointData.CheckerCount == 0)
            //{
            //    SendToPool();
            //    return;
            //}

       
            //To Do need to implement an object pool

            for (int i = 0; i < PointData.CheckerCount; ++i)
            {
                Beed tempBeed = null;
                if (PointData.Color == PlayerColors.White)
                {
                    tempBeed = TableManager.Instance.WhiteBeads.GetFromPull();
                }else
                {
                    tempBeed = TableManager.Instance.BlackBeads.GetFromPull();

                }

                pointBeeds.Push(tempBeed);
                tempBeed.transform.SetParent(this.transform);
                tempBeed.transform.position = FindPosition(i);
                tempBeed.GetComponent<Beed>().ID = PointData.ID;
                tempBeed.GetComponent<Beed>().Index = Index;
            }

        }

        public void SendToPool()
        {
            //To Do Use object pool insted of destroying game object
            if (pointBeeds.Count != 0)
            {
                for (int i = 0; i < pointBeeds.Count; ++i)
                {
                    if (pointBeeds.Peek().BeedColor == PlayerColors.White)
                        TableManager.Instance.WhiteBeads.SendToPool(pointBeeds.Pop());
                    else
                        TableManager.Instance.BlackBeads.SendToPool(pointBeeds.Pop());

                    --i;
                }
                
            }
        }

#if UNITY_EDITOR
        GameObject WhiteBeed = null;
        private void OnDrawGizmos()
        {

            if (WhiteBeed == null )
            {
                
                WhiteBeed = (GameObject)Resources.Load("Prefabs/WhiteBead");
            }

            if (WhiteBeed != null && sprite == null)
                sprite = WhiteBeed.GetComponent<SpriteRenderer>();

            if (sprite == null)
                return;

            //Gizmos.DrawWireCube(this.transform.position, PointBond);
            Gizmos.DrawSphere(new Vector3(this.transform.position.x, BeedStartPositionY, 0), sprite.sprite.bounds.extents.x);

            for (int i = 0; i < 8; ++i)
            {
                float yOffset = PointVisualizerSide == Side.UP ? (BeedStartPositionY - (sprite.sprite.bounds.size.x * i)) : BeedStartPositionY + ((sprite.sprite.bounds.size.x * i));
                Gizmos.DrawWireSphere(new Vector3(this.transform.position.x, yOffset, 0), sprite.sprite.bounds.extents.x);
            }

        }
#endif
    }
}