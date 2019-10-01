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

        private Stack<GameObject> pointBeeds = new Stack<GameObject>();
        private SpriteRenderer sprite;
        private static GameObject WhiteBeed = null;
        public static GameObject BlackBeed = null;

        private void Awake()
        {
            WhiteBeed = GameResourceManager.Instance.LoadPrefab("WhiteBead");
            BlackBeed = GameResourceManager.Instance.LoadPrefab("BlackBead");
            sprite = WhiteBeed.GetComponent<SpriteRenderer>();
            PointVisualizerManager.Instance.OnInitialDataSet += OnInitialDataSet;
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
            float offset = sprite.sprite.bounds.size.x;
            float yPosition = PointVisualizerSide == Side.UP ? BeedStartPositionY - (sprite.sprite.bounds.size.x * (Count))
                : BeedStartPositionY + (sprite.sprite.bounds.size.x * (Count));
            return new Vector2(this.transform.position.x, yPosition);
        }

        private void OnInitialDataSet()
        {
            if (PointData.CheckerCount == 0)
                return;
            GameObject go = PointData.Color == PlayerColors.White ? WhiteBeed : BlackBeed;

            for (int i = 0; i < PointData.CheckerCount; ++i)
            {
                GameObject tempBeed = null;
                pointBeeds.Push(tempBeed = Instantiate(go, Vector3.zero, Quaternion.identity));
                tempBeed.transform.SetParent(this.transform);
                tempBeed.transform.position = FindPosition(i);
                tempBeed.GetComponent<Beed>().ID = PointData.ID;
            }

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

            Gizmos.DrawWireCube(this.transform.position, PointBond);
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