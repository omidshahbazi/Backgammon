using ClientUtilities.ResourceManager;
using Simulation.Data.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{

    public class BarOff : MonoBehaviour
    {
        public enum Side
        {
            Down,
            UP
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


        public Stack<GameObject> pointBeeds
        {
            get;
            set;
        }

        private SpriteRenderer sprite;
        private static GameObject WhiteBeed = null;
        public static GameObject BlackBeed = null;

        private void Awake()
        {
            pointBeeds = new Stack<GameObject>();
            WhiteBeed = GameResourceManager.Instance.LoadPrefab("WhiteBead");
            BlackBeed = GameResourceManager.Instance.LoadPrefab("BlackBead");
            sprite = WhiteBeed.GetComponent<SpriteRenderer>();
            PointVisualizerManager.Instance.OnUpdatePointsData += OnUpdatePointsData;

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
            float offset = sprite.sprite.bounds.size.x;
            float yPosition = BarSide == Side.UP ? BeedStartPosition - (sprite.sprite.bounds.size.x * (Count))
                : BeedStartPosition + (sprite.sprite.bounds.size.x * (Count));
            return new Vector2(this.transform.position.x, yPosition);
        }

        private void OnUpdatePointsData()
        {

            if (BarCheckerCount == 0)
            {
                SendToPool();
                return;
            }

            GameObject go = Color == PlayerColors.White ? WhiteBeed : BlackBeed;
            //To Do need to implement an object pool

            for (int i = 0; i < BarCheckerCount; ++i)
            {

                GameObject tempBeed = null;
                pointBeeds.Push(tempBeed = Instantiate(go, Vector3.zero, Quaternion.identity));
                tempBeed.transform.SetParent(this.transform);
                tempBeed.transform.position = FindPosition(i);
                //tempBeed.GetComponent<Beed>().ID = PointData.ID;
                //tempBeed.GetComponent<Beed>().Index = Index;
            }

        }

        private void SendToPool()
        {

            if (pointBeeds.Count != 0)
            {
                for (int i = 0; i < pointBeeds.Count; ++i)
                {
                    Destroy(pointBeeds.Pop());
                    --i;
                }
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
            Gizmos.DrawSphere(new Vector3(this.transform.position.x, BeedStartPosition, 0), sprite.sprite.bounds.extents.x);

            for (int i = 0; i < 8; ++i)
            {
                float yOffset = BarSide == Side.UP ? (BeedStartPosition - (sprite.sprite.bounds.size.x * i)) : BeedStartPosition + ((sprite.sprite.bounds.size.x * i));
                Gizmos.DrawWireSphere(new Vector3(this.transform.position.x, yOffset, 0), sprite.sprite.bounds.extents.x);
            }

        }
#endif
    }
}
