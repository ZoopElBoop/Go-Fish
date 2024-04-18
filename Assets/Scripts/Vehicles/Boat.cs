using UnityEngine;

public class Boat : MonoBehaviour
{
    private Rigidbody rb;
    private BuoyancyObject bo;

    public float _Speed;
    [SerializeField] private Camera _boatCam;
    [SerializeField] private Transform _playerBoatPos;
    [SerializeField] private GameObject _boatCanvas;
    private Vector3 startingPos;
    private Quaternion startingRot;

    private bool isActive = false;
    private bool playerNear = true;

    private GameObject Player;
    private Transform playerReturnPos;

    private void Start()
    {
        playerReturnPos = GameObject.FindWithTag("Player Boat Exit").GetComponent<Transform>();
        Player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody>();
        bo = GetComponent<BuoyancyObject>();

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    private void Update()
    {
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
                ExitBoat();

            transform.Rotate(0f, Input.GetAxis("Horizontal"), 0f, Space.Self);


        }else if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterBoat();
        }
        FlipBoat();
    }

    private void FixedUpdate()
    {
        if (isActive && Input.GetAxis("Vertical") != 0.0f)
            rb.AddForce(GetMoveForce(), ForceMode.Force);
    }

    private Vector3 GetMoveForce() 
    {
        Vector3 moveForce = (_Speed * 1000f * Input.GetAxis("Vertical") * Time.fixedDeltaTime * transform.forward);

        moveForce *= PointsOnWater();

        return moveForce;
    }

    private float PointsOnWater()
    {
        return bo.floatingPointsUnderwater / bo._floatingPoint.Count;
    }

    private void FlipBoat()
    {
        if (PointsOnWater() == 1f)  //boat only flips itself back if fully in water
        {
            if (transform.eulerAngles.z >= 45f && transform.eulerAngles.z <= 315f)
            {
                Quaternion rotationEnd = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, rotationEnd, Time.deltaTime * 1);
            }
        }
    }

    private void EnterBoat() 
    {
        SwitchStates(false);

        Fishing fs = PlayerScriptManager.Instance.GetScript("Fishing");

        fs.boatRB = rb;

        Player.transform.SetPositionAndRotation(_playerBoatPos.position, _playerBoatPos.rotation);

        Player.transform.parent = transform;

        EventManager.Instance.BoatEnter(_boatCam);
    }

    private void ExitBoat()
    {
        SwitchStates(true);

        Player.transform.parent = null;

        print(Player.transform.position);
        print(playerReturnPos.transform.position);


        Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);

        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(startingPos, startingRot);

        EventManager.Instance.BoatExit();
    }

    private void SwitchStates(bool status)
    {
        PlayerScriptManager.Instance.ShutDown("Controller", status);
        PlayerScriptManager.Instance.ShutDown("Movement", status);
        PlayerScriptManager.Instance.ShutDown("Interact", status);

        GameManager.Instance.ShowPlayerMouse(!status);

        isActive = !status;
        _boatCam.enabled = !status;

        _boatCanvas.SetActive(status);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerNear)
        {
            playerNear = true;
            print(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerNear)
            playerNear = false;       
    }
}
