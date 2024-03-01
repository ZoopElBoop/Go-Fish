using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishControl : MonoBehaviour
{
    private Rigidbody rb;
    private bool beSpinning = false;
    private bool beAttracted = false;

    private Transform attractPoint;

    public FishData Data;

    [HideInInspector] public Transform _playerPos;
    [HideInInspector] public float _destroyRange;
    [HideInInspector] public int _dataIndex;

    public int HP;
    public float Speed;

    public float HeightMax;
    public float DepthMax;

    public bool canBeFished;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Data = FishDataManager.Instance.fishData[_dataIndex];

        HP = Data._Health;
        Speed = Data._Speed;

        HeightMax = Data._moveHeightLimit;
        DepthMax = Data._moveDepthLimit;

        canBeFished = Data._canBeCaught;

        ChangeDirection(30f, 180f);
        rb.velocity = transform.forward * 2;
    }

    private void FixedUpdate()
    {
        if (beSpinning)
            transform.Rotate(0f, 5f, 0f, Space.Self);
        else
            rb.velocity = transform.forward * Speed;

        if (transform.position.y > HeightMax || transform.position.y < DepthMax)
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
/*
        if (beAttracted)
            transform.LookAt(new Vector3(attractPoint.position.x, attractPoint.position.y, attractPoint.position.z));*/
    }

    private void LateUpdate()
    {
        var distanceBetween = Vector3.Distance(transform.position, _playerPos.position);

        if (distanceBetween > _destroyRange)
        {
            DIEFISHDIE();
            print("Fish OBLITERATED: Despawned");
        }

        if (HP <= 0)
        {
            DIEFISHDIE();
            print("Fish OBLITERATED: Killed");
        }
    }

    private void ChangeDirection(float xRange, float yRange) 
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x + Random.Range(-xRange, xRange), transform.eulerAngles.y + Random.Range(-yRange, yRange), 0f);    //requires overhaul for x rotation later on
    }

    void DIEFISHDIE() 
    {
        EventManager.Instance.FishCaught(gameObject);
    }
    private Vector3 aaaa;
    public void Attract(Transform focusPos) 
    {
        attractPoint = focusPos;
        aaaa = focusPos.position;

        transform.LookAt(new Vector3(attractPoint.position.x, attractPoint.position.y, attractPoint.position.z));

        print("fish be lookin " + focusPos.position);
    }
    public void Flee(Transform focusPos) 
    {
        transform.LookAt(new Vector3(transform.position.x - focusPos.position.x, transform.position.z - focusPos.position.y, transform.position.z - focusPos.position.z));
        ChangeDirection(30f,60f);

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
        if (attractPoint != null)
            Gizmos.DrawCube(aaaa, new Vector3(1f, 1f, 1f));
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
