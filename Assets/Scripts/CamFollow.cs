using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public GameObject target;
    public float xOffset;
    public float yOffset;
    public float zOffset;
    public bool fixedRotation;
    public float xRotation;
    public float yRotation;
    public float zRotation;

    void Update()
    {
        transform.position = new Vector3(target.transform.position.x + xOffset, target.transform.position.y + yOffset, target.transform.position.z + zOffset);

        if (fixedRotation)
            transform.eulerAngles = new Vector3(xRotation,yRotation,zRotation);
        else
            transform.rotation = target.transform.rotation;
    }
}
