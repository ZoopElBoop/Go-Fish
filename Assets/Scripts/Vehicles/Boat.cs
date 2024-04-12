using UnityEngine;

public class Boat : MonoBehaviour
{
    private Rigidbody rb;
    private BuoyancyObject bo;

    public float _Speed;
    [SerializeField] private Camera _boatCam;
    [SerializeField] private Transform _playerBoatPos;
    private Vector3 startingPos;
    private Quaternion startingRot;

    private bool isActive = false;
    private bool playerLeft = true;
    public bool onWater = false;

    private GameObject Player;
    private Transform playerReturnPos;

    private void Start()
    {
        playerReturnPos = GameObject.FindWithTag("Player Boat Exit").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        bo = GetComponent<BuoyancyObject>();

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    private void Update()
    {
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))           
                ExitBoat();

            transform.Rotate(0f, Input.GetAxis("Rotate"), 0f, Space.Self);

            
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

                transform.rotation = Quaternion.Slerp(transform.rotation, rotationEnd, Time.deltaTime);
            }
        }
    }

    private void ExitBoat()
    {
        Player.transform.parent = null;

        Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);

        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(startingPos, startingRot);

        PlayerScriptManager.Instance.ShutDown("Controller", true);
        PlayerScriptManager.Instance.ShutDown("Movement", true);
        PlayerScriptManager.Instance.ShutDown("Interact", true);

        GameManager.Instance.ShowPlayerMouse(false);

        isActive = false;
        _boatCam.enabled = false;

        EventManager.Instance.BoatExit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && playerLeft)
        {
            Player = other.gameObject;

            PlayerScriptManager.Instance.ShutDown("Controller", false);
            PlayerScriptManager.Instance.ShutDown("Movement", false);
            PlayerScriptManager.Instance.ShutDown("Interact", false);

            GameManager.Instance.ShowPlayerMouse(true);

            Fishing fs = PlayerScriptManager.Instance.GetScript("Fishing");

            fs.boatRB = rb;

            Player.transform.SetPositionAndRotation(_playerBoatPos.position, _playerBoatPos.rotation);

            Player.transform.parent = transform;

            isActive = true;
            _boatCam.enabled = true;

            EventManager.Instance.BoatEnter(_boatCam);

            playerLeft = false;
        }

        if (other.CompareTag("Water"))
            onWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !playerLeft)
            playerLeft = true;       
    }
}
