using System;
using UnityEngine;

public class Tide : MonoBehaviour
{
    [Range(0f, 2f)] public float tideMax;
    [Min(0)] public float tideTime;

    private Vector3 startingPos;
    private bool movingUp = false;

    void Start()
    {
        startingPos = transform.position;
    }

    void Update()
    {
        Vector3 endPos = new Vector3(0f, tideMax, 0f) + startingPos;

        if (movingUp)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, Time.deltaTime * tideTime);

            if (transform.position.y == endPos.y)
                movingUp = false;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startingPos, Time.deltaTime * tideTime);

            if (transform.position.y == startingPos.y)
                movingUp = true;
        }
    }      
}
