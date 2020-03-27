using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class UIObjectRotator : MonoBehaviour
{
    RectTransform rectTrans;

    public float Speed;

    private void Awake()
    {
        rectTrans = transform.GetComponent<RectTransform>();

    }

    void Update()
    {
        transform.Rotate(0, 0, 180.0F * Speed * Time.deltaTime);
    }
}
