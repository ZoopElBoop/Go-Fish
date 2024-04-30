using System.Collections;
using UnityEngine;

public class Sub : MonoBehaviour
{
    public GameObject subCanvas;

    [Header("Sub Move & Rotate Multipliers")]
    public float _Speed = 1f;
    private float floatBase;

    private bool playerNear;
    private bool isActive = false;

    [Header("Sub Cameras")]
    public Camera _subOuterCam;
    public Camera _subInnerCam;

    [Header("Harpoon Spawning")]
    public GameObject Harpoon;
    public Transform _harpoonSpawn;
    private bool canFire = true;

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
        fishSpawner = GameObject.FindWithTag("Spawner");
        Player = GameObject.FindWithTag("Player").transform.root.gameObject;

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
            LiftOrDescend();

            SwitchCams();

            ExitSub();

            HarpoonFire();

            transform.Rotate(0f, Input.GetAxis("Horizontal"), 0f, Space.Self);

        }
        else if (playerNear && Input.GetKeyDown(KeyCode.E) && !GameManager.Instance.InVessel)
            EnterSub();
    }

    private void FixedUpdate()
    {
        if (isActive && Input.GetAxis("Vertical") != 0.0f)
            rb.AddForce(GetMoveForce(), ForceMode.Force);
    }

    private Vector3 GetMoveForce()
    {
        Vector3 moveForce = (_Speed * 1000f * Input.GetAxis("Vertical") * Time.fixedDeltaTime * transform.forward);

        moveForce *= bo.floatingPointsUnderwater / bo._floatingPoint.Count;

        return moveForce;
    }

    private void LiftOrDescend()
    {
        if (Input.GetAxis("Lift") > 0.0f && bo._floatingPower < floatBase)
            bo._floatingPower += 0.1f;
        else if ((Input.GetAxis("Lift") < 0.0f && bo._floatingPower > 0))
            bo._floatingPower -= 0.1f;

        if (Input.GetKeyDown(KeyCode.H))
            bo._floatingPower = 9.81f;
    }

    private void EnterSub() 
    {
        subCanvas.SetActive(false);

        GameManager.Instance.SetVesselStatus(true);
        PlayerScriptManager.Instance.ShutDown("Interact", false);

        SetFishSpawner(transform);

        Player.SetActive(false);

        isActive = true;
        _subOuterCam.enabled = true;
    }

    private void ExitSub()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            subCanvas.SetActive(true);

            GameManager.Instance.SetVesselStatus(false);
            PlayerScriptManager.Instance.ShutDown("Interact", true);

            SetFishSpawner(Player.transform);

            Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);
            Player.SetActive(true);

            isActive = false;
            _subOuterCam.enabled = false;
            _subInnerCam.enabled = false;

            rb.velocity = Vector3.zero;
            transform.SetPositionAndRotation(startingPos, startingRot);
        }
    }
    private void SetFishSpawner(Transform setTo)
    {
        fishSpawner.transform.position = setTo.position;
        fishSpawner.transform.parent = setTo;
    }

    private void SwitchCams() 
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            _subOuterCam.enabled = !_subOuterCam.enabled;
            _subInnerCam.enabled = !_subInnerCam.enabled;
        }
    }

    private void HarpoonFire()
    {
        if (Input.GetMouseButtonDown(0) && canFire)
        {
            StartCoroutine(FireCooldown());
            GameManager.Instance.SpawnHarpoon(Harpoon, _harpoonSpawn.position, _harpoonSpawn.rotation);
            rb.velocity = transform.forward * -3f;
        }
    }

    IEnumerator FireCooldown() 
    {
        canFire = false;
        yield return new WaitForSeconds(3f);
        canFire = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerNear)
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerNear)
            playerNear = false;
    }
}