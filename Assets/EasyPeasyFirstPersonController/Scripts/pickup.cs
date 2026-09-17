

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pickup : MonoBehaviour
{
    private float mouseZValue;
    [SerializeField] bool FreezeRotation;


    Rigidbody rb;
    private Vector3 objectPosition;


    Camera cam;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }


    void OnMouseDown()
    {
        mouseZValue = cam.WorldToScreenPoint(gameObject.transform.position).z;
        objectPosition = gameObject.transform.position - TranslateMousePos();
    }




    private Vector3 TranslateMousePos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mouseZValue;
        return cam.ScreenToWorldPoint(mousePoint);
    }




    void OnMouseDrag()
    {
        transform.position = TranslateMousePos() + objectPosition;
        if (FreezeRotation)
        {
            rb.freezeRotation = true;
        }
    }


    private void OnMouseUp()
    {
        if (FreezeRotation)
        {
            rb.freezeRotation = false;
        }
    }
}
