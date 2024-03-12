using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishControl : MonoBehaviour
{
    private Rigidbody rb;
    private bool beSpinning = false;
    private bool beAttracted = false;

    private Transform attractPoint;
    public Transform rotatePos;

    [Header("Fish Data")]
    public FishData Data;

    [HideInInspector] public Transform _playerPos;
    [HideInInspector] public float _destroyRange;
    [HideInInspector] public int _dataIndex;

    [SerializeField] private float HeightMax;
    [SerializeField] private float DepthMax;

    public int HP;
    public float Speed;
    public float rotationSpeed;

    public bool canBeFished;
    private bool isTurning;

    [Header("Collision Detection Settings")]

    //public List<Collider> meshies = new();

    public bool canCheckCollisions = true;
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

    void Start()
    {
        viewCollisionBox = false;

        rb = GetComponent<Rigidbody>();
        /*

                Data = FishDataManager.Instance.fishData[_dataIndex];

                HP = Data._Health;
                Speed = Data._Speed;
                rotationSpeed = Data._rotationSpeed;

                HeightMax = Data._moveHeightLimit;
                DepthMax = Data._moveDepthLimit;

                canBeFished = Data._canBeCaught;

                */

        //transform.eulerAngles = new Vector3(transform.eulerAngles.x + Random.Range(-30, 30), transform.eulerAngles.y + Random.Range(-180, 180), 0f);

        /*       for (int i = 0; i < transform.childCount; i++)
        {
            Collider childCollider = transform.GetChild(i).GetComponent<MeshCollider>();

            if (childCollider != false)
                meshies.Add(childCollider);
        }*/

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        collisionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        collisionBox.layer = LayerIgnoreRaycast;
        collisionBox.SetActive(false);
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * Speed;
        /*
        if (beSpinning)
            transform.Rotate(0f, 5f, 0f, Space.Self);
        else
            rb.velocity = transform.forward * Speed;*//*

                if (transform.position.y > HeightMax || transform.position.y < DepthMax)
                    transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);*/
        /*        if (canCheckCollisions)
                    CollisionDetect();*/

        ChangeDirection();

        CollisionDetect();

        //var distanceBetween = Vector3.Distance(transform.position, _playerPos.position);
        /*
                float distanceBetween = (_playerPos.position - transform.position).sqrMagnitude;    //this might be more efficent than Vector3.Distance since it doesn't do any square rooting

                if (distanceBetween > _destroyRange * _destroyRange)                                //squared to make up for no square rooting in previous line
                {
                    DIEFISHDIE();
                    print("Fish OBLITERATED: Despawned");
                }
        */
    }

    private void Update()
    {
/*        if (HP <= 0)
        {
            DIEFISHDIE();
            print("Fish OBLITERATED: Killed");
        }
        */
    }
    public List<int> posClearIndex = new();
    private void CollisionDetect()
    {
        Vector3[] Positions = {
            transform.position + (transform.forward * colliderRange),                                            //front
            transform.position + (transform.forward * colliderRange) + (transform.right * colliderSize.x),       //right
            transform.position + (transform.forward * colliderRange) + (-transform.right * colliderSize.x),      //left
            transform.position + (transform.forward * colliderRange) + (transform.up * colliderSize.y),          //top-centre
            transform.position + (transform.forward * colliderRange) + (-transform.up * colliderSize.y)          //bottom-centre
        };

        posClearIndex.Clear();

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

            if (i == 3 && transform.position.y + colliderSize.y > HeightMax || i == 4 && transform.position.y - colliderSize.y < DepthMax)
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
            RotateTo(new Vector3(transform.position.x - Positions[0].x, transform.position.y - Positions[0].y, 0f));
            return;
        }

        int arrSize = posClearIndex.Count;

        for (int i = 0; i < arrSize; i++)
        {
            if (rayDistance[i] == -1)
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
            return (-1, Vector3.zero);
        }
        else if (hits > 1)
        {
            colliderFound = RaycastArraySort(colliderFound);

            for (int i = 0; i < colliderFound.Length; i++)           
                if (colliderFound[i].distance != 0)                
                    return (colliderFound[i].distance, colliderFound[i].point);                    
        }
        
        return (colliderFound[0].distance, colliderFound[0].point);  
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


    private bool CanMoveToPos(Vector3 endPosition)
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

    private void RotateTo(Vector3 rotationTarget)       //Rotates player to point
    {
        initialAngle.x = Mathf.DeltaAngle(0, transform.eulerAngles.x);
        initialAngle.y = transform.eulerAngles.y;

        Vector3 relativePos = rotationTarget - transform.position; 

        rotationEnd = Quaternion.LookRotation(relativePos, Vector3.up);

        newAngle = new(
            Mathf.DeltaAngle(0, rotationEnd.eulerAngles.x),
            rotationEnd.eulerAngles.y,
            0f);

        isTurning = true;
    }

    void DIEFISHDIE()
    {
        if (collisionBox != null)
            Destroy(collisionBox);      //TBC
        EventManager.Instance.FishCaught(gameObject);
    }

    public void Attract(Transform focusPos)
    {
        attractPoint = focusPos;

        RotateTo(focusPos.position);

        print("fish be lookin " + focusPos.position);

        StartCoroutine(AttractReset());
    }

    IEnumerator AttractReset() 
    {
        yield return new WaitForSeconds(0.1f);
        //yeild return new WaitForSeconds(1f);
        
    }

    public void Flee(Transform focusPos)
    {
        RotateTo(new Vector3(transform.position.x - focusPos.position.x, transform.position.y - focusPos.position.y, 0f));
        //ChangeDirection(30f, 60f);

        print("fish be runnin");
    }

    public void Caught()
    {
        //Logic for when fishing rod catches fish

        print("i be catght");

        beSpinning = true;

        rb.velocity = Vector3.zero;
    }

    public void Escape(Transform focusPos)
    {
        //Logic for when fish escapes from caught event

        print("ran awaaaaaaaaaaaaaaaaa");
        beSpinning = false;

        rb.velocity = Vector3.zero;

        Flee(focusPos);
    }

    private void OnDrawGizmosSelected()
    {
        if (viewCollisionBox)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(Vector3.forward * colliderRange, colliderSize / 1.05f);

        }
    }
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