using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub : MonoBehaviour
{
    [Header("Sub Move & Rotate Multipliers")]
    public float _Speed = 1f;
    public float _RotationSpeed = 1f;
    private float floatBase;

    private bool isActive = false;

    [Header("Sub Cameras")]
    public Camera _subOuterCam;
    public Camera _subInnerCam;

    [Header("Harpoon Spawning")]
    public GameObject Harpoon;
    public Transform _harpoonSpawn;
    public int _harpoonSpawnLimit;
    public int _harpoonSpawnCount;

    private Transform playerReturnPos;
    private GameObject fishSpawner;
    private GameObject Player;

    private Vector3 startingPos;
    private Quaternion startingRot;

    private Rigidbody rb;
    private BuoyancyObject bo;

    private void Start()
    {
        playerReturnPos = GameObject.FindGameObjectWithTag("Player Boat Exit").GetComponent<Transform>();
        fishSpawner = GameObject.FindGameObjectWithTag("Spawner");
        rb = GetComponent<Rigidbody>();
        bo = GetComponent<BuoyancyObject>();

        floatBase = bo._floatingPower;

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.GetAxis("Vertical") != 0.0f)
                rb.AddForce(_Speed * 1000f * Input.GetAxis("Vertical") * Time.deltaTime * transform.forward, ForceMode.Force);

            if (Input.GetAxis("Lift") > 0.0f && bo._floatingPower < floatBase)
                bo._floatingPower += 0.1f;
            else if ((Input.GetAxis("Lift") < 0.0f && bo._floatingPower > 0))
                bo._floatingPower -= 0.1f;

            transform.Rotate(0f, Input.GetAxis("Rotate") / 2 * _RotationSpeed, 0f, Space.Self);

            if (Input.GetKeyDown(KeyCode.C))
                SwitchCams();

            if (Input.GetKeyDown(KeyCode.Space))           
                PlayerExit();

            if (Input.GetMouseButtonDown(0) && _harpoonSpawnCount < _harpoonSpawnLimit)
                HarpoonFire();
            
            if (transform.eulerAngles.z >= 15f && transform.eulerAngles.z <= 350f)
            {
                Quaternion rotationEnd = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, rotationEnd, 5 * Time.deltaTime);
            }
        }
    }

    private void PlayerExit() 
    {
        SetFishSpawner(Player.transform);

        Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);
        Player.SetActive(true);

        isActive = false;
        _subOuterCam.enabled = false;
        _subInnerCam.enabled = false;

        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(startingPos, startingRot);
    }

    private void SwitchCams() 
    {
        if (_subOuterCam.enabled)
        {
            _subOuterCam.enabled = false;
            _subInnerCam.enabled = true;
        }
        else
        {
            _subOuterCam.enabled = true;
            _subInnerCam.enabled = false;
        }
    }

    private void SetFishSpawner(Transform setTo) 
    {
        fishSpawner.transform.position = setTo.position;
        fishSpawner.transform.parent = setTo;
    }

    private void HarpoonFire()
    {
        _harpoonSpawnCount++;
        Instantiate(Harpoon, _harpoonSpawn.position, _harpoonSpawn.rotation);
        rb.velocity = transform.forward * -3f;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player = other.gameObject;

            SetFishSpawner(transform);

            Player.SetActive(false);

            isActive = true;
            _subOuterCam.enabled = true;
        }
    }
}