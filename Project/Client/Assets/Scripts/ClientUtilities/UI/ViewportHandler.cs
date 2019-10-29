
using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ViewportHandler : MonoBehaviour
{

    public Vector2 ReferenceRatio = new Vector2(9f, 16f);
    private float fixedOthographicSize = 5.44F;
    private  Camera camera =null;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        camera.orthographicSize = fixedOthographicSize;
        ComputeCameraBound();
    }

    private void ComputeCameraBound()
    {

        float  num1= Screen.width / (float)Screen.height;
        float num2 = this.ReferenceRatio.x / this.ReferenceRatio.y;

        float num3 = num2 / num1;

        if (num1 >= num2)
            return;

        camera.orthographicSize = fixedOthographicSize* num3;
       
    }
    
}