using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class RopeChain : MonoBehaviour
{
    private LineRenderer Line;

    [Header("End Of Line")]
    public Transform EndPoint;

    [Header("Line Settings")]
    [Min(2)] public int numberOfPoints = 17;
    public float Drop = 1f;

    private float distanceBetween;
    private readonly float timePoints = 0.01f;

    [Header("Objects Settings")]
    public bool ObjectsOnLine = true;

    [Space]
    public GameObject lineObjectPrefab;
    [SerializeField] private List<GameObject> lineObjects = new();  //needs to be serialised as to save data between playing exiting,
                                                                    //hidden as to not throw the ObjectDisposedException as unity in this version is abit bugged 
    [Range (1, 100)] public int numberOfObjects = 1;
    public Vector3 positionOffset;

    [Header("Objects Rotation Settings")]
    public Vector3 rotationOffset;
    [Space]
    public bool randomObjectRotation = false;
    public bool reRoll = false;
    public RandomRotationAxis randomRotationAxis;

    [Header("Advanced Settings")]
    public bool DoNotRespawn = false;
    public bool IgnoreNullObjects = false;
    public bool ControlRotationFromPivot = false;
    public bool viewGizmos = false;

    [System.Flags]
    public enum RandomRotationAxis
    {
        None = 0,
        x = 1,
        y = 2,
        z = 4   //DO NOT CHANGE TO 3, SINCE ITS BASED ON BINARY NOTATION 3 WILL EQUAL x & y !!!!
    }

    private void Start()
    {
        Line = GetComponent<LineRenderer>();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        enabled = false;
    }

    private void Update() 
    {
        if (Line != null && EndPoint != null)
        {
            RenderLine();

            if (ObjectsOnLine && lineObjectPrefab != null)
                PutObjectsOnLine();
            else
                ClearAnyObjects(0);
        }
    }

    private void RenderLine() 
    {
        Line.positionCount = numberOfPoints;

        Line.SetPosition(0, transform.position);
        Line.SetPosition(numberOfPoints - 1, EndPoint.position);

        distanceBetween = (float) 1 / (numberOfPoints - 1);
        int midPoint = Mathf.CeilToInt(numberOfPoints / 2);

        if (Line.positionCount > 2)
        {
            Vector3 velocity = Drop * -transform.up;
            float time = 0f;

            for (int i = 1; i < numberOfPoints - 1; i++)
            {
                if (i < midPoint)
                    time += Mathf.InverseLerp(midPoint, 0f, i) * timePoints;
                else
                    time -= Mathf.InverseLerp(midPoint, numberOfPoints + 1, i) * timePoints;

                var y = (velocity.y * time) + (Physics.gravity.y / 2 * time * time);

                Vector3 point = Vector3.Lerp(transform.position, EndPoint.position, distanceBetween * i);

                point = new Vector3(
                    point.x,
                    point.y + y,
                    point.z
                    );

                Line.SetPosition(i, point);
            }
        }
    }

    private void PutObjectsOnLine() 
    {
        if (!DoNotRespawn)
            for (int i = lineObjects.Count; i < numberOfObjects; i++)
                lineObjects.Add(Instantiate(lineObjectPrefab, transform));

        if (lineObjects.Count > numberOfObjects)
            ClearAnyObjects(numberOfObjects);

        float distanceBetween = (float) 1 / (numberOfObjects + 1);

        for (int i = 0; i < lineObjects.Count; i++)
        {
            if (lineObjects[i] == null)     //for if ingorenull is enabled
                continue;

            lineObjects[i].transform.SetPositionAndRotation(
                FindPointOnLine(distanceBetween * (i + 1)), 
                SetRotation(i));
        }

        if (randomObjectRotation)
            reRoll = true;
    }

    private Vector3 FindPointOnLine(float pointOnLine)
    {
        //Find what 2 points box lies between
        //Inverse lerp to find where box is between points (x & z only)
        //Use value to lerp y coords between points

        Vector3 point = Vector3.Lerp(transform.position, EndPoint.position, pointOnLine);

        float pointTo = 0;
        int startingPointIndex = 0;

        while (pointTo < pointOnLine)
        {
            pointTo += distanceBetween;
            startingPointIndex++;
        }

        float pointOnRange = Vector3InverseLerpIngoreY(Line.GetPosition(startingPointIndex - 1), Line.GetPosition(startingPointIndex), point);

        point = new Vector3(
            point.x,
            Vector3.Lerp(Line.GetPosition(startingPointIndex - 1), Line.GetPosition(startingPointIndex), pointOnRange).y,
            point.z
            );

        return point + positionOffset;
    }

    private Quaternion SetRotation(int index) 
    {
        if (ControlRotationFromPivot)
            return transform.rotation;

        Vector3 newAngle = lineObjects[index].transform.eulerAngles;

        if (randomObjectRotation && !reRoll)
        {
            float x = randomRotationAxis.HasFlag(RandomRotationAxis.x) ? Random.Range(-180f, 180f) : 0;
            float y = randomRotationAxis.HasFlag(RandomRotationAxis.y) ? Random.Range(-180f, 180f) : 0;
            float z = randomRotationAxis.HasFlag(RandomRotationAxis.z) ? Random.Range(-180f, 180f) : 0;

            newAngle = new Vector3(x, y, z);
        }
        else if (!randomObjectRotation)      
            newAngle = rotationOffset;

        return Quaternion.Euler(newAngle);
    }

    private void ClearAnyObjects(int from)
    {
        if (lineObjects != null)
            for (int i = from; i < lineObjects.Count; i++)
                DestroyImmediate(lineObjects[i]);

        if (!IgnoreNullObjects || !ObjectsOnLine)
            lineObjects.RemoveAll(s => s.Equals(null));
    }

    //return point where vector lies on ignoring y axcis
    private float Vector3InverseLerpIngoreY(Vector3 a, Vector3 b, Vector3 value)
    {
        a.y = 0f;
        b.y = 0f;
        value.y = 0f;

        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    private void OnDrawGizmosSelected()
    {
        if (viewGizmos)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < numberOfPoints; i++)
                Gizmos.DrawWireSphere(Line.GetPosition(i), 0.25f);

            Gizmos.color = Color.red;

            for (int i = 0; i < lineObjects.Count; i++)
            {
                if (lineObjects[i] != null)
                    Gizmos.DrawWireSphere(lineObjects[i].transform.position, 1f);
            }
        }
    }
}
