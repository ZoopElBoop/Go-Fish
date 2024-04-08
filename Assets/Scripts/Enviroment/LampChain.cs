using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LampChain : MonoBehaviour
{    
    public LineRenderer Line;
    public Transform to;
    public Transform faaaaaarom;

    [Min(2)] public int numberOfPoints;

    public float speed;

    private void Update() 
    {
        if (Line != null && to != null)
            RenderLine();
    }

    private void RenderLine() 
    {
        Line.positionCount = numberOfPoints;

        Line.SetPosition(0, transform.position);
        Line.SetPosition(numberOfPoints - 1, to.position);

        if (Line.positionCount == 2)
            return;

        float distanceBetween = (float) 1 / (numberOfPoints - 1);

        Vector3 velocity = speed * -transform.up;
        float time = 0f;

        for (int i = 1; i < numberOfPoints - 1; i++)
        {
            var y = (velocity.y * time) + (Physics.gravity.y / 2 * time * time);

            Vector3 point = GetPoint(transform.position, to.position, distanceBetween * i);

            point = new Vector3(
                point.x,
                point.y - 2f,
                point.z
                );

            Line.SetPosition(i, point);
            faaaaaarom.position = point;
        }
    }

    private Vector3 GetPoint(Vector3 a, Vector3 b, float point) 
    {
        return a + point * (b - a);
    }
}
