using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CollisionDetect))]
[RequireComponent(typeof(Rigidbody))]
public class FishControl : MonoBehaviour
{
    #region Variables

    public FishData Data;
    private CollisionDetect AI;
    private Rigidbody rb;

    private Transform attractPoint;
    
    [HideInInspector] public bool isAboutToDie = false;
    [HideInInspector] public bool canBeFished;

    //Fish Spawner
    private Transform spawnPosition;
    [HideInInspector] public float _destroyRange;

    //Move Limits
    private float HeightMax;
    private float DepthMax;

    [Header("Fish Base Values")]

    public int HP;
    public float Speed;
    private float rotationSpeed;

    //Fish To Player Variables
    private Vector3 startingScale;
    private Vector3 startingPoint;
    private float pointInTravel = 1f;

    [Header("Harpoons Attached")]
    public List<GameObject> harpoonsAttached = new();

    public AK.Wwise.Event fishCollected;

    #endregion

    #region Start Functions

    void Awake()
    {
        spawnPosition = GameObject.FindWithTag("Spawner").transform;

        rb = GetComponent<Rigidbody>();
        startingScale = transform.localScale;
    }

    private void OnEnable()
    {
        isAboutToDie = false;

        HP = Data._Health;
        Speed = Data._Speed;
        rotationSpeed = Data._rotationSpeed;

        HeightMax = Data._moveHeightLimit;
        DepthMax = Data._moveDepthLimit;

        canBeFished = Data._canBeCaught;

        transform.localScale = startingScale;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x + Random.Range(-30, 30), transform.eulerAngles.y + Random.Range(-180, 180), 0f);

        AI = GetComponent<CollisionDetect>();
        AI.SetMoveValues(HeightMax, DepthMax, rotationSpeed);
    }

    #endregion

    #region Update Functions

    private void FixedUpdate()
    {
        if (!isAboutToDie)
            rb.velocity = transform.forward * Speed;
    }

    private void Update()
    {
        float distanceBetween = (spawnPosition.position - transform.position).sqrMagnitude;    //this might be more efficent than Vector3.Distance since it doesn't do any square rooting

        if (!isAboutToDie)
        {
            if (distanceBetween > _destroyRange * _destroyRange)                                //squared to make up for no square rooting in previous line        
                DIEFISHDIE();

            if (HP <= 0)
            {
                InventoryManager.Instance.StoreOnSub(this);

                ActivateFishToPlayer();
            }

        }
        else
            FishToPlayer(distanceBetween);
    }

    #endregion

    #region Fish States
    void DIEFISHDIE()
    {
        GameManager.Instance.DestroyFish(gameObject);
    }

    public void Attract(Transform focusPos)
    {
        attractPoint = focusPos;

        AI.RotateTo(focusPos.position, true);

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
        AI.RotateTo(new Vector3(transform.position.x - focusPos.position.x, transform.position.y - focusPos.position.y, 0f), false);

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
        EventManager.Instance.FishCaught();
        AI.enabled = false;

        attractPoint = null;
        rb.velocity = Vector3.zero;
        startingPoint = transform.position;

        canBeFished = false;
        isAboutToDie = true;
    }

    private void FishToPlayer(float distanceBetween)
    {
        if (pointInTravel > 0.25)
            pointInTravel = Mathf.Clamp01(Vector3InverseLerp(spawnPosition.position, startingPoint, transform.position));

        if (distanceBetween > 2)
        {
            transform.Rotate(0f, 5f, 0f, Space.Self);
            transform.localScale = new Vector3(pointInTravel, pointInTravel, pointInTravel);
            transform.position = Vector3.Slerp(transform.position, spawnPosition.position, Time.deltaTime);
        }
        else
        {
            fishCollected.Post(gameObject);
            DIEFISHDIE();
        }
    }

    private float Vector3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    private void OnDisable()
    {
        if (harpoonsAttached != null)
        {
            foreach (var harpoon in harpoonsAttached)
                GameManager.Instance.DestroyHarpoon(harpoon);

            harpoonsAttached.Clear();
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