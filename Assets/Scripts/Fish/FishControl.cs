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
    public Vector3 colliderSize;
    public float colliderRange;
    private GameObject collisionBox;

    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    private Quaternion rotationEnd;

    private List<Collider> meshies = new();

    [Header("DEBUG")]
    public bool ba;
    public bool viewCollisionBox;
    public bool[] isHit = new bool[5]; //0 - front, 1 - right, 2 - left, 3 - top, 4 - bottom



    void Start()
    {
        //viewCollisionBox = false;

        rb = GetComponent<Rigidbody>();
        /*

                Data = FishDataManager.Instance.fishData[_dataIndex];

                HP = Data._Health;
                Speed = Data._Speed;
                rotationSpeed = Data._rotationSpeed;

                HeightMax = Data._moveHeightLimit;
                DepthMax = Data._moveDepthLimit;

                canBeFished = Data._canBeCaught;

                ChangeDirection(30f, 180f);*/

        for (int i = 0; i < transform.childCount; i++)
        {
            Collider childCollider = transform.GetChild(i).GetComponent<MeshCollider>();

            if (childCollider != false)
                meshies.Add(childCollider);
        }

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        print(transform.childCount);

        collisionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        collisionBox.layer = LayerIgnoreRaycast;
        collisionBox.SetActive(false);
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * Speed;
        if (beSpinning)
            transform.Rotate(0f, 5f, 0f, Space.Self);
        else
            rb.velocity = transform.forward * Speed;/*

                if (transform.position.y > HeightMax || transform.position.y < DepthMax)
                    transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);*/

        if (!ba)
            CollisionDetect();

        ChangeDirection();
    }
    
    private void LateUpdate()
    {
        //var distanceBetween = Vector3.Distance(transform.position, _playerPos.position);
/*
        var distanceBetween = (_playerPos.position - transform.position).sqrMagnitude;  //this might be more efficent than Vector3.Distance since it doesn't do any square rooting

        if (distanceBetween > _destroyRange * _destroyRange)                            //squared to make up for no square rooting in previous line
        {
            DIEFISHDIE();
            print("Fish OBLITERATED: Despawned");
        }

        if (HP <= 0)
        {
            DIEFISHDIE();
            print("Fish OBLITERATED: Killed");
        }*/
    }

    private void CollisionDetect()
    {
        Vector3[] Positions = {
            transform.position + (transform.forward * colliderRange),                                                   //front
            transform.position + (transform.forward * colliderRange) + (transform.right * colliderSize.x) * 1.5f,       //right
            transform.position + (transform.forward * colliderRange) + (-transform.right * colliderSize.x) * 1.5f,      //left
            transform.position + (transform.forward * colliderRange) + (transform.up * colliderSize.y) * 1.5f,          //top-centre
            transform.position + (transform.forward * colliderRange) + (-transform.up * colliderSize.y) * 1.5f          //bottom-centre
        };

        List<int> posClearIndex = new();

        for (int x = 0; x < 5; x++)
            isHit[x] = false;

        for (int i = 0; i < 5; i++)
        {
            Collider[] itemsReturned = CheckPositionForColliders(Positions[i]);

            if (i == 0 && itemsReturned[0] == null)     //Checks if front is clear, if so prevents other checks as not required.
            {
                isHit[i] = false;
                return;
            }

            if (i == 3 && transform.position.y + colliderSize.y > HeightMax || i == 4 && transform.position.y - colliderSize.y < DepthMax)
            {
                print(i + "ignored");
                isHit[i] = true;
                continue;
            }

            if (itemsReturned[0] != null)
            {
                isHit[i] = true;
            }
            else
            {
                isHit[i] = false;

                if (CanMoveToPos(Positions[i]))
                    posClearIndex.Add(i);
                else
                    Debug.DrawLine(transform.position, Positions[i], Color.red, 4f);       
            }
        }

        ba = true;

        if (!posClearIndex.Any())
        {
            Debug.Log("nah");
            return;
        }

        for (int i = 0; i < posClearIndex.Count; i++)      
            Debug.DrawLine(transform.position, Positions[posClearIndex[i]], Color.black, 2f); 

        int randPick = Random.Range(0, posClearIndex.Count);

        Debug.DrawLine(transform.position, Positions[posClearIndex[randPick]], Color.green, 2f);

        RotateTo(Positions[posClearIndex[randPick]]);

        if (viewCollisionBox)
        {
            collisionBox.SetActive(true);

            collisionBox.transform.position = (Positions[posClearIndex[randPick]] + transform.position) / 2;

            Vector3 targetDir = Positions[posClearIndex[randPick]] - transform.position;

            var angle = Quaternion.LookRotation(targetDir, transform.up);

            collisionBox.transform.rotation = angle;

            collisionBox.transform.localScale = new Vector3(colliderSize.x, colliderSize.y, colliderRange);
        }else
            collisionBox.SetActive(false);

        print("picked " + randPick);
    }

    private Collider[] CheckPositionForColliders(Vector3 positionToCheck)
    {
        Collider[] hitColliders = new Collider[10];

        Physics.OverlapBoxNonAlloc(positionToCheck, colliderSize / 2, hitColliders, transform.rotation, LayersToIgnore,  QueryTriggerInteraction.Ignore);

        return hitColliders;
    }

    private bool CanMoveToPos( Vector3 endPosition)
    {

        Vector3 targetDir = endPosition - transform.position;

        var angle = Quaternion.LookRotation(targetDir, transform.up);

        //IgnoreColliders(false);

        Collider[] hitColliders = new Collider[10];

        int Hits = Physics.OverlapBoxNonAlloc((endPosition + transform.position) / 2, new Vector3(colliderSize.x, colliderSize.y, colliderRange) / 2, hitColliders, angle, LayersToIgnore, QueryTriggerInteraction.Ignore);

        //IgnoreColliders(true);

        if (hitColliders[0] == null)
            return true; 

        for (int i = 0; i < Hits; i++)
        {
            if (hitColliders[i] != null)
                print(hitColliders[i].name);
        }

        print("-----------------");

        return false;


        /*        float rayDistance = Vector3.Distance(transform.position, endPosition);

                RaycastHit[] hits;

                hits = Physics.RaycastAll(transform.position, (endPosition - transform.position), rayDistance);

                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)   //to ignore fish colliders, really jank ik but it be worky, as long as each part of fish have collider < 1f away from origin
                        if (hits[i].distance > 1f)
                            return false;
                }
                return true;*/
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

    private void ChangeDirection()
    {
        if (isTurning)
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationEnd, rotationSpeed * Time.deltaTime);

        if (isTurning && transform.rotation == rotationEnd)
        {
            print("aaa");
            isTurning = false;
            ba = false;

            //Debug.Break();
        }
    }

    private void RotateTo(Vector2 rotationAngle) 
    {
        Vector3 rotationFixedAngle = new (
            Mathf.Clamp(rotationAngle.x, -30, 30),
            Mathf.Clamp(rotationAngle.y, -100, 100),
            0f);

        print(rotationAngle);

        rotationEnd = Quaternion.Euler(rotationAngle.x, rotationAngle.y, rotationFixedAngle.z);
        print(rotationEnd);

        isTurning = true;
    }

    private void RotateTo(Vector3 rotationTarget)
    {
        Vector3 relativePos = rotationTarget - transform.position; 

        rotationEnd = Quaternion.LookRotation(relativePos, Vector3.up);
        print(rotationEnd);

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

        transform.LookAt(attractPoint.position);

        print("fish be lookin " + focusPos.position);
    }

    public void Flee(Transform focusPos)
    {
        RotateTo(new Vector3(transform.position.x - focusPos.position.x, transform.position.z - focusPos.position.y, transform.position.z - focusPos.position.z));
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

            if (isHit[0])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube(Vector3.forward * colliderRange, colliderSize / 1.05f);

            if (isHit[1])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube((Vector3.forward * colliderRange) + (Vector3.right * colliderSize.x) * 1.5f, colliderSize / 1.05f);

            if (isHit[2])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube((Vector3.forward * colliderRange) + (-Vector3.right * colliderSize.x) * 1.5f, colliderSize / 1.05f);

            if (isHit[3])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube((Vector3.forward * colliderRange) + (Vector3.up * colliderSize.y) * 1.5f, colliderSize / 1.05f);

            if (isHit[4])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawWireCube((Vector3.forward * colliderRange) + (-Vector3.up * colliderSize.y) * 1.5f, colliderSize / 1.05f);

            if (isHit[4])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            //Gizmos.DrawWireCube((Vector3.forward * colliderRange) + (-Vector3.up * colliderSize) + (-Vector3.right * colliderSize), Vector3.one * (colliderSize - 0.25f));
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
