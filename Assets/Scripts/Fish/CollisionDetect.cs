using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionDetect : MonoBehaviour
{
    [Header("Collision Detection Settings")]

    //public List<Collider> meshies = new();

    //Collision Check States
    private bool canCheckCollisions = true;
    private bool overrideFrontCheck = false;

    //Ray Layer Mask
    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    //Front Colliders Values
    public Vector3 colliderSize;
    public float colliderRange;
    public float largeColliderRange;

    //Ray Constants
    private readonly float RAY_CHECK_DISTANCE = 50f;
    private readonly float RAY_MINIMUM_DISTANCE = 5f;

    [Header("Rotation Values")]
    public Quaternion rotationEnd;
    public Vector3 initialAngle = new();
    public Vector3 newAngle;

    //Move state & direction
    private bool isTurning;
    private int moveDirection;

    //Values From Fish Data
    private float rotationSpeed;
    private float maxHeight;
    private float maxDepth;

    //Rotation Limit
    private readonly float X_ROTATION_LIMIT = 80f;

    [Header("DEBUG")]
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
        CheckForObjects();

        CheckLimits();

        ChangeDirection();
    }

    #region Collision Checks

    private void CheckForObjects()
    {
        /*
        Collision detect has 3 seperate systems that operate depending on the fishes position

        1. Fish Front Detection
            Every 2ms a box check is made in front of the fish, if nothing is found the detection is complete,
            If something is found the function will determine the direction to rotate in if it hasn't already been done in the previous frame,
            If the direction has already been d function calls a new Rotate() action with the already defined direction.

        2. Ray Checks
            Four rays are fired in front of the fish, each returns the distance & closest point of contact with a collider (if possible),
            Rays are checked to ensure they don't exceed fish height & depth limits and that they are above a minimum distance,
            Rays that pass this check have their data passed further on and a check is made to find the furthest ray contact,
            If two or more rays have the same highest distance one of the rays will be randomly selected.

        3. Rotation
            After the correct direction is found the angle of movement is converted to a quaternion (kill me) and the fish is lerped to the new rotation,
            If the extended fish front check is still detecting something the fish will continue roatating in this direction.
        
        ISSUES:
            This is terrible, awful infact, kinda works but i am not good at ai stuff :(
        */

        Vector3 FrontPosition = transform.position + (transform.forward * colliderRange);

        Vector3[] CheckPositions = {
            transform.position + (transform.forward * colliderRange) + (transform.right * colliderSize.x),       //right 
            transform.position + (transform.forward * colliderRange) + (-transform.right * colliderSize.x),      //left 
            transform.position + (transform.forward * colliderRange) + (transform.up * colliderSize.y),          //top 
            transform.position + (transform.forward * colliderRange) + (-transform.up * colliderSize.y),         //bottom 
        };

        Vector3[] ForwardClearPositionAndSize = {
            transform.position + (transform.forward * (colliderSize.z / 2 + largeColliderRange / 2 + colliderRange)),   //postion of collider
            new Vector3(colliderSize.x, colliderSize.y, largeColliderRange)                                             //size of collider
        };


        if (!overrideFrontCheck && CheckPositionForColliders(FrontPosition, colliderSize))  //checks if area is clear 
        {
            canCheckCollisions = true;
            return;
        }
        else if (!canCheckCollisions)
        {
            SetRotation();

            if (CheckPositionForColliders(ForwardClearPositionAndSize[0], ForwardClearPositionAndSize[1]))
                overrideFrontCheck = false;

            return;
        }


        canCheckCollisions = false;
        overrideFrontCheck = true;

        float[] rayDistance = new float[4];
        Vector3[] rayPoint = new Vector3[4];

        List<int> positionsClearIndex = new();


        for (int i = 0; i < 4; i++)
        {
            (rayDistance[i], rayPoint[i]) = DistanceFromContact(CheckPositions[i] - transform.position);

            if (i == 2) 
                if (HitHeighLimit(colliderSize.y) || HitRotationUpLimit())
                    continue;

            if (i == 3)
                if (HitDepthLimit(colliderSize.y) || HitRotationDownLimit())
                    continue;

            if (rayDistance[i] == -1 || rayDistance[i] >= RAY_MINIMUM_DISTANCE)
                positionsClearIndex.Add(i);
        }


        if (!positionsClearIndex.Any())   //rotates fish 180 if no suitable directions found
        {
            RotateTo(new Vector3(transform.position.x - FrontPosition.x, transform.position.y - FrontPosition.y, 0f), false);
            return;
        }

        List<int> finalPositions = new();
        int arrSize = positionsClearIndex.Count;

        for (int i = 0; i < arrSize; i++)   //finds ray(s) that have no distance or the highest distance
        {
            if (rayDistance[positionsClearIndex[i]] == -1)       //adds rays with no hits recorded
            {
                finalPositions.Add(positionsClearIndex[i]);
                continue;
            }

            for (int x = 0; x < arrSize; x++)
            {
                Debug.DrawLine(transform.position, CheckPositions[positionsClearIndex[i]], Color.black, 5f);

                if (rayDistance[positionsClearIndex[i]] < rayDistance[positionsClearIndex[x]] || rayDistance[x] == -1)
                    break;

                if (x == arrSize - 1)
                    finalPositions.Add(positionsClearIndex[i]); //sets to array if after comparision with all objec
            }
        }

        if (!finalPositions.Any())   //rotates fish 180 if no suitable directions found
        {
            RotateTo(new Vector3(transform.position.x - FrontPosition.x, transform.position.y - FrontPosition.y, 0f), false);
            return;
        }

        for (int i = 0; i < finalPositions.Count; i++)
            Debug.DrawLine(transform.position, CheckPositions[finalPositions[i]], Color.black, 5f);

        int randPick = Random.Range(0, finalPositions.Count);

        moveDirection = finalPositions[randPick];

        Debug.DrawLine(transform.position, CheckPositions[finalPositions[randPick]], Color.green, 5f);
    }

    private bool CheckPositionForColliders(Vector3 positionToCheck, Vector3 size)   //returns true if nothing found
    {
        Collider[] hitColliders = new Collider[1];

        Physics.OverlapBoxNonAlloc(positionToCheck, size / 2, hitColliders, transform.rotation, LayersToIgnore, QueryTriggerInteraction.Ignore);

        return hitColliders[0] == null;
    }

    private (float distanceTo, Vector3 contactPoint) DistanceFromContact(Vector3 dir)
    {
        Ray ray = new(transform.position, dir);

        RaycastHit[] colliderFound = new RaycastHit[10];

        int hits = Physics.RaycastNonAlloc(ray, colliderFound, RAY_CHECK_DISTANCE, LayersToIgnore, QueryTriggerInteraction.Ignore);

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

        Collider[] hitColliders = new Collider[10];

        int Hits = Physics.OverlapBoxNonAlloc((endPosition + transform.position) / 2, new Vector3(colliderSize.x, colliderSize.y, colliderRange) / 2, hitColliders, angle, LayersToIgnore, QueryTriggerInteraction.Ignore);

        if (hitColliders[0] == null)
            return true;

        if (viewCollisionBox)
        {
            for (int i = 0; i < Hits; i++)
            {
                if (hitColliders[i] != null)
                    print("HIT:" + hitColliders[i].name);
            }
        }
        return false;
    }


    #endregion

    #region Rotation Functions

    private void SetRotation()
    {
        switch (moveDirection)
        {
            case 0:
                RotateTo(Vector2.up);
                break;
            case 1:
                RotateTo(-Vector2.up);
                break;
            case 2:
                RotateTo(-Vector2.right);
                break;
            case 3:
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

            transform.localRotation = Quaternion.Slerp(transform.rotation, rotationEnd, turnSpeed * Time.deltaTime);
        }
    }

    private void ResetRotation()    //Resets objects x rotation
    {
        initialAngle.x = Mathf.DeltaAngle(0, transform.eulerAngles.x);
        initialAngle.y = transform.eulerAngles.y;

        newAngle = new(
            0f,
            Mathf.Clamp(newAngle.y, -360, 360),
            0f);

        rotationEnd = Quaternion.Euler(newAngle);

        isTurning = true;
    }


    public void RotateTo(Vector2 rotationAngle)    //Rotates object by set angle values
    {
        initialAngle.x = Mathf.DeltaAngle(0, transform.eulerAngles.x);
        initialAngle.y = transform.eulerAngles.y;

        newAngle = initialAngle + new Vector3(rotationAngle.x, rotationAngle.y, 0);

        newAngle = new(
            Mathf.Clamp(newAngle.x, -X_ROTATION_LIMIT, X_ROTATION_LIMIT),
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

    #region Limit Checks

    private void CheckLimits() 
    {
        if (HitDepthLimit(0f) || HitHeighLimit(0f))
            ResetRotation();
    }

    private bool HitRotationUpLimit() => Mathf.Approximately(Mathf.DeltaAngle(0, transform.eulerAngles.x), -X_ROTATION_LIMIT);

    private bool HitRotationDownLimit() => Mathf.Approximately(Mathf.DeltaAngle(0, transform.eulerAngles.x), X_ROTATION_LIMIT);

    private bool HitHeighLimit(float buffer) => transform.position.y + buffer > maxHeight;      //has buffer so rays dont check up/down when near limits

    private bool HitDepthLimit(float buffer) => transform.position.y - buffer < maxDepth;

    #endregion

    #region Gizmo

    private void OnDrawGizmosSelected()
    {
        if (viewCollisionBox)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(Vector3.forward * colliderRange, colliderSize);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.forward * (colliderSize.z/2 + largeColliderRange/2 + colliderRange), new Vector3(colliderSize.x, colliderSize.y, largeColliderRange));
        }
    }

    #endregion
}
