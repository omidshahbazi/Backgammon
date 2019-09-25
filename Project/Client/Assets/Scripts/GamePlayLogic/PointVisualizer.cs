using Simulation.Common;
using Simulation.Data.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameObject Beed;
    public PointData Data
    {
        get;
        set;
    }

    private CircleCollider2D Collider;


    private void Start()
    {
        Collider = Beed.GetComponent<CircleCollider2D>();
    }

    private Vector2 FindEmptyPosition()
    {
        float yPosition = PointVisualizerSide == Side.UP ? BeedStartPositionY - (Collider.radius * 2 * (Data.CheckerCount + 1)) 
            : BeedStartPositionY + (Collider.radius * 2 * (Data.CheckerCount + 1));
        return new Vector2(this.transform.position.x, yPosition);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Beed == null)
            return;
        if (Collider == null)
            Collider = Beed.GetComponent<CircleCollider2D>();

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
