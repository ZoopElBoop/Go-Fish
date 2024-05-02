using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    [Header("Collision Detection Settings")]

    //public List<Collider> meshies = new();

    private bool canCheckCollisions = true;

    //Front Collider Vaues
    public Vector3 colliderSize;
    public float colliderRange;

    //Front Collider Full Values
    //public float colliderFullSize;

    private int moveDirection;

    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    [Header("Rotation Values")]
    public Quaternion rotationEnd;
    public Vector3 initialAngle = new();
    public Vector3 newAngle;

    public float rayMinDistance;
    private bool isTurning;

    private float rotationSpeed;

    public float maxHeight;
    public float maxDepth;

    public bool viewCollisionBox;

    private void Start()
    {
        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        LayerIgnoreRaycast = LayerMask.NameToLayer("Water");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "water" layer
    }

    public void SetMoveValues(float height, float depth, float speed) 
    {
        maxHeight = height;
        maxDepth = depth;
        rotationSpeed = speed;
    }

    private void FixedUpdate()
    {
        ChangeDirection();
        CheckForObjects();
    }

    #region Collision Checks

    private void CheckForObjects()
    {
        /*
        Collision detect has 3 seperate systems that operate depending on the fishes position

        1. Fish Front Detection
            Every 2ms a box check is made in front of the fish, if nothing is found the detection is complete,
            If something is found the function will commit to the next system if it hasn't already been called in the previous frame,
            If the next system has already been called the function calls a new Rotate() action with the already defined direction.

        2. Ray Checks
            Four rays are fired in front of the fish, each returns the distance & closest point of contact with a collider (if possible),
            Rays are checked to ensure they don't exceed fish height & depth limits and that they are above a minimum distance,
            Rays that pass this check have their data passed further on and a check is made to find the furthest ray contact,
            If two or more rays have the same highest distance one of the rays will be randomly selected.

        3. Rotation
            After the correct direction is found the angle of movement is converted to a quaternion (kill me) and the fish is lerped to the new rotation,
            If the fish front check is still detecting something the fish will continue roatating in this direction.
        
        ISSUES:

            Fish cannot turn fast enough in tight corners.
            If the collision box is detecting, a new rotation direction cannot be applied.
        */

        Vector3[] Positions = {
            transform.position + (transform.forward * colliderRange),                                            //front
            transform.position + (transform.forward * colliderRange) + (transform.right * colliderSize.x),       //right
            transform.position + (transform.forward * colliderRange) + (-transform.right * colliderSize.x),      //left
            transform.position + (transform.forward * colliderRange) + (transform.up * colliderSize.y),          //top-centre
            transform.position + (transform.forward * colliderRange) + (-transform.up * colliderSize.y)          //bottom-centre
        };

        List<int> posClearIndex = new();

        if (CheckPositionForColliders(Positions[0]))
        {
            canCheckCollisions = true;
            return;
        }
        else if (!canCheckCollisions)
        {
            SetRotation();
            return;
        }

        float[] rayDistance = new float[4];
        Vector3[] rayPoint = new Vector3[4];

        for (int i = 0; i < 4; i++)
        {
            (rayDistance[i], rayPoint[i]) = DistanceFromContact(Positions[i + 1] - transform.position);

            if (i == 3 && transform.position.y > maxHeight || i == 4 && transform.position.y < maxDepth)
            {
                print(i + "ignored");
                continue;
            }

            if (rayDistance[i] == -1 || rayDistance[i] >= rayMinDistance)
            {
                if (true)//CanMoveToPos(rayPoint[i]))
                {
                    posClearIndex.Add(i);
                    //Debug.DrawLine(transform.position, Positions[i], Color.green, 4f);
                }
            }
        }

        canCheckCollisions = false;

        if (!posClearIndex.Any())   //rotates fish 180 if no suitable directions found
        {
            RotateTo(new Vector3(transform.position.x - Positions[0].x, transform.position.y - Positions[0].y, 0f), false);
            return;
        }

        int arrSize = posClearIndex.Count;

        for (int i = 0; i < arrSize; i++)   //finds ray(s) that have no distance or the highest distance
        {
            if (rayDistance[i] == -1)       //ignores rays with no hits recorded
                continue;

            for (int x = 0; x < arrSize; x++)
            {
                if (rayDistance[i] < rayDistance[x] || rayDistance[x] == -1)
                {
                    print("REMOVED: " + rayDistance[i]);
                    posClearIndex.Remove(i);
                    break;
                }
            }
        }

        for (int i = 0; i < posClearIndex.Count; i++)
            Debug.DrawLine(transform.position, Positions[posClearIndex[i] + 1], Color.black, 5f);

        int randPick = Random.Range(0, posClearIndex.Count);

        moveDirection = posClearIndex[randPick];

        SetRotation();

        print("picked " + randPick);
    }

    private bool CheckPositionForColliders(Vector3 positionToCheck)
    {
        Collider[] hitColliders = new Collider[1];

        Physics.OverlapBoxNonAlloc(positionToCheck, colliderSize / 2, hitColliders, transform.rotation, LayersToIgnore, QueryTriggerInteraction.Ignore);

        return hitColliders[0] == null;
    }

    private (float distanceTo, Vector3 contactPoint) DistanceFromContact(Vector3 dir)
    {
        Ray ray = new(transform.position, dir);

        RaycastHit[] colliderFound = new RaycastHit[10];

        int hits = Physics.RaycastNonAlloc(ray, colliderFound, 30f, LayersToIgnore, QueryTriggerInteraction.Ignore);

        if (hits < 1)
        {
            return (-1, Vector3.zero);                                              //Ray hit nothing
        }
        else if (hits > 1)
        {
            colliderFound = RaycastArraySort(colliderFound);                        //sorts RaycastHit array by smallest distance

            for (int i = 0; i < colliderFound.Length; i++)
                if (colliderFound[i].distance != 0)
                    return (colliderFound[i].distance, colliderFound[i].point);     //Ray hit more than 1 item, sorted to item closest to player                 
        }

        return (colliderFound[0].distance, colliderFound[0].point);                 //Ray hit 1 item
    }

    private RaycastHit[] RaycastArraySort(RaycastHit[] arr)      //sorts array by distance
    {
        for (int i = 1; i < arr.Length; i++)
        {
            RaycastHit key = arr[i];
            int j = i - 1;

            while (j >= 0 && arr[j].distance > key.distance)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = key;
        }
        return arr;
    }

    private bool CanMoveToPosition(Vector3 endPosition)
    {
        Vector3 targetDir = endPosition - transform.position;

        Quaternion angle = Quaternion.LookRotation(targetDir, transform.up);

        //IgnoreColliders(false);

        Collider[] hitColliders = new Collider[10];

        int Hits = Physics.OverlapBoxNonAlloc((endPosition + transform.position) / 2, new Vector3(colliderSize.x, colliderSize.y, colliderRange) / 2, hitColliders, angle, LayersToIgnore, QueryTriggerInteraction.Ignore);

        //IgnoreColliders(true);

        if (hitColliders[0] == null)
            return true;

        if (viewCollisionBox)
        {
            for (int i = 0; i < Hits; i++)
            {
                if (hitColliders[i] != null)
                    print("HIT:" + hitColliders[i].name);
            }

            print("-----------------");
        }

        return false;
    }

    /*    private void IgnoreColliders(bool reset)
        {
            for (int i = 0; i < meshies.Count; i++)
            {
                if (!reset)
                    meshies[i].gameObject.layer = LayerIgnoreRaycast;
                else
                    meshies[i].gameObject.layer = 0;
            }
            print(meshies[0].gameObject.layer);
        }*/

    #endregion

    #region Rotation Functions

    private void SetRotation()
    {
        switch (moveDirection + 1)
        {
            case 1:
                RotateTo(Vector2.up);
                break;
            case 2:
                RotateTo(-Vector2.up);
                break;
            case 3:
                RotateTo(-Vector2.right);
                break;
            case 4:
                RotateTo(Vector2.right);
                break;
        }
    }

    private void ChangeDirection()
    {
        if (isTurning && transform.rotation == rotationEnd)
            isTurning = false;

        if (isTurning)
        {
            float turnSpeed;

            if ((initialAngle - newAngle).sqrMagnitude <= 1)
                turnSpeed = 100f;
            else
                turnSpeed = rotationSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation, rotationEnd, turnSpeed * Time.deltaTime);
        }
    }

    public void RotateTo(Vector2 rotationAngle)    //Rotates object by set angle values
    {
        initialAngle.x = Mathf.DeltaAngle(0, transform.eulerAngles.x);
        initialAngle.y = transform.eulerAngles.y;

        newAngle = initialAngle + new Vector3(rotationAngle.x, rotationAngle.y, 0);

        newAngle = new(
            Mathf.Clamp(newAngle.x, -80, 80),
            Mathf.Clamp(newAngle.y, -360, 360),
            0f);

        rotationEnd = Quaternion.Euler(newAngle);

        isTurning = true;
    }

    public void RotateTo(Vector3 rotationTarget, bool checkArea)       //Rotates object to point
    {
        initialAngle.x = Mathf.DeltaAngle(0, transform.eulerAngles.x);
        initialAngle.y = transform.eulerAngles.y;

        if (checkArea && !CanMoveToPosition(rotationTarget))
        {
            print("cannot reach");
            return;
        }

        Vector3 relativePos = rotationTarget - transform.position;

        rotationEnd = Quaternion.LookRotation(relativePos, Vector3.up);

        newAngle = new(
            Mathf.DeltaAngle(0, rotationEnd.eulerAngles.x),
            rotationEnd.eulerAngles.y,
            0f);

        isTurning = true;
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmosSelected()
    {
        if (viewCollisionBox)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(transform.forward * colliderRange, colliderSize);

            //Gizmos.color = Color.red;
            //Gizmos.DrawWireCube(transform.forward * colliderRange, new Vector3(colliderSize.x, colliderSize.y, colliderSize.z + colliderFullSize));
        }
    }

    #endregion

}
