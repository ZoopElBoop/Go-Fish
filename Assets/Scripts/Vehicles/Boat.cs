using UnityEngine;

public class Boat : MonoBehaviour
{
    private Rigidbody rb;

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

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    void Update()
    {
        if (isActive)
        {
            if (onWater)
            {
                if (Input.GetAxis("Vertical") != 0.0f)
                    rb.AddForce(_Speed * 10000f * Input.GetAxis("Vertical") * Time.deltaTime * transform.forward, ForceMode.Force);

                transform.Rotate(0f, Input.GetAxis("Rotate"), 0f, Space.Self);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.transform.parent = null;

                Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);

                rb.velocity = Vector3.zero;
                transform.SetPositionAndRotation(startingPos, startingRot);

                PlayerScriptManager.Instance.ShutDown("Controller", true);
                PlayerScriptManager.Instance.ShutDown("Movement", true);

                isActive = false;
                _boatCam.enabled = false;

                EventManager.Instance.BoatExit();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && playerLeft)
        {
            Player = other.gameObject;

            PlayerScriptManager.Instance.ShutDown("Controller", false);
            PlayerScriptManager.Instance.ShutDown("Movement", false);

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

        if (other.CompareTag("Water"))
            onWater = false;
    }
}
