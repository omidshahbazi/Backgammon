//using Simulation.Common;
//using Simulation.Data.Game;
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
        public GameObject WhiteBeed;
        [SerializeField]
        public GameObject BlackBeed;
        [SerializeField]
        public Sprite HighlightHeleper;


        //public PointData PointData
        //{
        //    get;
        //    set;
        //}

        private CircleCollider2D Collider;


        //private void Start()
        //{
        //    Collider = WhiteBeed.GetComponent<CircleCollider2D>();
        //    PointVisualizerManager.Instance.OnInitialDataSet += OnInitialDataSet;

        //}

     

        //public Vector2[] FindPositions()
        //{
        //    if (PointData.CheckerCount == 0)
        //        return null;

        //    List<Vector2> list = new List<Vector2>();
        //    for (int i = 0; i < PointData.CheckerCount; ++i)
        //        list.Add(FindPosition(i));

        //    return list.ToArray();
        //}

        ////Always you have should send count+1 if you want find empty space
        //public Vector2 FindPosition(int Count)
        //{
        //    float yPosition = PointVisualizerSide == Side.UP ? BeedStartPositionY - (Collider.radius * 2 * (Count))
        //        : BeedStartPositionY + (Collider.radius * 2 * (Count));
        //    return new Vector2(this.transform.position.x, yPosition);
        //}

        //private void OnInitialDataSet()
        //{
        //    if (PointData.CheckerCount == 0)
        //        return;


        //}




#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (WhiteBeed == null)
                return;
            if (Collider == null)
                Collider = WhiteBeed.GetComponent<CircleCollider2D>();

            if (Collider == null)
                return;

            Gizmos.DrawWireCube(this.transform.position, PointBond);
            Gizmos.DrawSphere(new Vector3(this.transform.position.x, BeedStartPositionY, 0), Collider.radius);

            for (int i = 0; i < 10; ++i)
            {
                float yOffset = PointVisualizerSide == Side.UP ? BeedStartPositionY - (Collider.radius * 2 * i) : BeedStartPositionY + (Collider.radius * 2 * i);
                Gizmos.DrawWireSphere(new Vector3(this.transform.position.x, yOffset, 0), Collider.radius);
            }

        }
#endif
    }
}