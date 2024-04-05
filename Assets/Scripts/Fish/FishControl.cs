using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishControl : MonoBehaviour
{
    #region Variables

    private Rigidbody rb;

    public bool isAboutToDie = false;

    private Transform attractPoint;

    [Header("Fish Data")]

    public Transform _playerPos;
    public float _destroyRange;
    public int _dataIndex;

    [SerializeField] private float HeightMax;
    [SerializeField] private float DepthMax;

    public int HP;
    public float Speed;
    public float rotationSpeed;

    public bool canBeFished;
    private bool isTurning;

    [Header("Collision Detection Settings")]

    //public List<Collider> meshies = new();

    private bool canCheckCollisions = true;
    public Vector3 colliderSize;
    public float colliderRange;
    private GameObject collisionBox;
    private int moveDirection;

    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    [Header("Rotation Values")]
    public Quaternion rotationEnd;
    public Vector3 initialAngle = new();
    public Vector3 newAngle;

    public float rayMinDistance;

    [Header("DEBUG")]
    public bool viewCollisionBox;

    //Fish To Player Variables
    public Vector3 startingScale;
    private Vector3 startingPoint;
    private float pointInTravel = 1f;

    #endregion

    #region Start Functions

    void Awake()
    {

        viewCollisionBox = false;

        rb = GetComponent<Rigidbody>();
        startingScale = transform.localScale;

        /*       for (int i = 0; i < transform.childCount; i++)
        {
            Collider childCollider = transform.GetChild(i).GetComponent<MeshCollider>();

            if (childCollider != false)
                meshies.Add(childCollider);
        }*/

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        LayerIgnoreRaycast = LayerMask.NameToLayer("Water");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "water" layer

        /*        collisionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                collisionBox.layer = LayerIgnoreRaycast;
                collisionBox.SetActive(false);*/
    }

    private void OnEnable()
    {
        isAboutToDie = false;
        isTurning = false;

        transform.localScale = startingScale;

        HP = FishDataManager.Instance.GetHealth(_dataIndex);
        Speed = FishDataManager.Instance.GetSpeed(_dataIndex);
        rotationSpeed = FishDataManager.Instance.GetRotationSpeed(_dataIndex);

        HeightMax = FishDataManager.Instance.GetHeightLimit(_dataIndex);
        DepthMax = FishDataManager.Instance.GetDepthLimit(_dataIndex);

        canBeFished = FishDataManager.Instance.GetCanBeCaught(_dataIndex);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x + Random.Range(-30, 30), transform.eulerAngles.y + Random.Range(-180, 180), 0f);
    }

    #endregion

    #region Update Functions

    private void FixedUpdate()
    {
        if (!isAboutToDie)
        {
            rb.velocity = transform.forward * Speed;

            ChangeDirection();

            //CollisionDetect();
        }
    }

    private void Update()
    {
        float distanceBetween = (_playerPos.position - transform.position).sqrMagnitude;    //this might be more efficent than Vector3.Distance since it doesn't do any square rooting

        if (!isAboutToDie)
        {
            if (transform.position.y > HeightMax || transform.position.y < DepthMax)
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

            if (distanceBetween > _destroyRange * _destroyRange)                                //squared to make up for no square rooting in previous line        
                DIEFISHDIE();


    /*        if (HP <= 0)
            {
                DIEFISHDIE();
                print("Fish OBLITERATED: Killed");
            }
*/
        }
        else
            FishToPlayer(distanceBetween);
    }

    #endregion

    #region Collision Detection Checks

    private void CollisionDetect()
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
        else
        {
            if (!canCheckCollisions)
            {
                SetRotation();
                return; 
            }
        }

        float[] rayDistance = new float[4];
        Vector3[] rayPoint = new Vector3[4];

        for (int i = 0; i < 4; i++)
        {
            (rayDistance[i], rayPoint[i]) = DistanceFromContact(Positions[i + 1] - transform.position);

            if (i == 3 && transform.position.y > HeightMax || i == 4 && transform.position.y < DepthMax)
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

        if (!posClearIndex.Any())
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

        //SetCollisionBox(rayPoint[randPick], rayDistance[randPick]);

        print("picked " + randPick);
    }

    private bool CheckPositionForColliders(Vector3 positionToCheck)
    {
        Collider[] hitColliders = new Collider[1];

        Physics.OverlapBoxNonAlloc(positionToCheck, colliderSize / 2, hitColliders, transform.rotation, LayersToIgnore,  QueryTriggerInteraction.Ignore);

        return hitColliders[0] == null;
    }

    private (float distanceTo, Vector3 contactPoint) DistanceFromContact(Vector3 dir) 
    {
        Ray ray = new(transform.position, dir);

        RaycastHit[] colliderFound = new RaycastHit[10];

        int hits = Physics.RaycastNonAlloc(ray, colliderFound, 30f, LayersToIgnore, QueryTriggerInteraction.Ignore);

        //Debug.DrawRay(transform.position, dir * 2, Color.red, Mathf.Infinity);

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

    private RaycastHit[] RaycastArraySort(RaycastHit[] arr)
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

    /*    private void SetCollisionBox(Vector3 pos, float distance) 
    {
        if (viewCollisionBox)
        {
            collisionBox.SetActive(true);

            collisionBox.transform.position = (pos + transform.position) / 2;

            Vector3 targetDir = pos - transform.position;

            Quaternion angle = Quaternion.LookRotation(targetDir, transform.up);

            collisionBox.transform.rotation = angle;

            collisionBox.transform.localScale = new Vector3(colliderSize.x, colliderSize.y, distance);
        }
        else
            collisionBox.SetActive(false);
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

    private void RotateTo(Vector2 rotationAngle)    //Rotates fish by set angle values
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

    private void RotateTo(Vector3 rotationTarget, bool checkArea)       //Rotates player to point
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

    #region Fish States
    void DIEFISHDIE()
    {
        if (collisionBox != null)
            Destroy(collisionBox);      //TBC

        GameManager.Instance.DestroyFish(gameObject);
    }

    public void Attract(Transform focusPos)
    {
        attractPoint = focusPos;

        RotateTo(focusPos.position, true);

        StartCoroutine(AttractReset());
    }

    IEnumerator AttractReset()    
    {
        yield return new WaitForSeconds(1f);

        if (attractPoint != null && (attractPoint.position - transform.position).sqrMagnitude > 25)     
            Attract(attractPoint);
    }

    public void Flee(Transform focusPos)
    {
        RotateTo(new Vector3(transform.position.x - focusPos.position.x, transform.position.y - focusPos.position.y, 0f), false);

        print($"fish be runnin {gameObject.name}");
    }

    public void Escape(Transform focusPos)
    {
        //Logic for when fish escapes from caught event

        print($"ran awaaaaaaaaaaaaaaaaa {gameObject.name}");

        rb.velocity = Vector3.zero;

        Flee(focusPos);
    }

    #endregion

    #region Fish To Player

    public void ActivateFishToPlayer()
    {
        attractPoint = null;
        rb.velocity = Vector3.zero;
        startingPoint = transform.position;
        print("ded");

        canBeFished = false;
        isAboutToDie = true;
    }

    private void FishToPlayer(float distanceBetween) 
    {
        if (pointInTravel > 0.25)
            pointInTravel = Mathf.Clamp01(Vector3InverseLerp(_playerPos.position, startingPoint, transform.position));

        if (distanceBetween > 2)
        {
            transform.Rotate(0f, 5f, 0f, Space.Self);
            transform.localScale = new Vector3(pointInTravel, pointInTravel, pointInTravel);
            transform.position = Vector3.Slerp(transform.position, _playerPos.position, Time.deltaTime);
        }
        else
            GameManager.Instance.DestroyFish(gameObject);
    }

    private float Vector3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmosSelected()
    {
        if (viewCollisionBox)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(Vector3.forward * colliderRange, colliderSize / 1.05f);
        }
    }

    #endregion
}

/*
    Fishstreet Boys - I want to fish.

    [Intro: AJ]
    Fish-yeah-yeah-yeah

    [Verse 1: Brian]
    You are my fish
    The one i eat
    Believe when I say
    I want to fish

    [Verse 2: Nick]
    But we are two lakes apart
    Can't reach to your fish heart
    When you say
    That I want to fish

    [Chorus: Nick & All]
    Let me fish
    Ain't nothin' but a fishcake
    Let me fish
    Ain't nothing but a fish-steak
    Let me fish
    I never wanna see you swim
    I want to fish

    [Verse 3: AJ, AJ & Brian]
    Am I your bait?
    Your one favorite mate
    Yes, I know it's too late
    But I want to fish
    [Chorus: AJ, All & Brian]
    Let me fish
    Ain't nothin' but a fishcake
    Let me fish
    Ain't nothing but a fish-steak
    Let me fish
    I never wanna see you swim
    I want to fish

    [Bridge: Kevin, Kevin & AJ]
    Now I can see that we're not fishing no more
    From the way that it used to be, yeah
    No matter the depth, I want you to know
    That i want to fish right now

    [Verse 4: Howie, Nick & All]
    You are my fish
    The one i eat
    My fish (My fish, My fish, My fish)
    Just wanna fish you

    [Break: All & Nick]
    Ain't nothin' but a fishcake (Say, hey, yeah)
    Ain't nothin' but a fish-steak (Don't wanna hear you fish)
    I never wanna see swim (Oh, yeah)
    I want to fish
    [Chorus: Brian, All, Nick & AJ]
    Let me fish
    Ain't nothin' but a fishcake
    Let me fish
    Ain't nothin' but a fish-steak
    Let me fish
    I never wanna see you swim (Don't wanna see you swim)
    I want to fish
    Let me fish
    Ain't nothin' but a fishcake
    Ain't nothin' but a fish-steak
    Let me fish
    I never wanna see you swim (Don't wanna see you swim, yeah)
    I want to fish

    [Outro: AJ]
    'Cause I want to fish
*/