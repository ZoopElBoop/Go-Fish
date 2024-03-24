using UnityEngine;

public class Boat : MonoBehaviour
{
    private Rigidbody rb;
    private CharacterController cc;
    private PlayerMovement pm;
    private Fishing fs;

    public float _Speed;
    [SerializeField] private Camera _boatCam;
    [SerializeField] private Transform _playerBoatPos;
    private Vector3 startingPos;
    private Quaternion startingRot;

    private bool isActive = false;
    private bool playerLeft = true;

    private GameObject Player;
    private Transform playerReturnPos;

    private void Start()
    {
        playerReturnPos = GameObject.FindGameObjectWithTag("Player Boat Exit").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.GetAxis("Vertical") != 0.0f)
                rb.AddForce(_Speed * 10000f * Input.GetAxis("Vertical") * Time.deltaTime * transform.forward, ForceMode.Force);

            transform.Rotate(0f, Input.GetAxis("Rotate"), 0f, Space.Self);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.transform.parent = null;

                Player.transform.SetPositionAndRotation(playerReturnPos.position, playerReturnPos.rotation);

                rb.velocity = Vector3.zero;
                transform.SetPositionAndRotation(startingPos, startingRot);

                cc.enabled = true;
                pm.enabled = true;

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

            cc = Player.GetComponent<CharacterController>();
            pm = Player.GetComponent<PlayerMovement>();
            fs = Player.GetComponent<Fishing>();

            cc.enabled = false;
            pm.enabled = false;

            fs.boatRB = rb;

            Player.transform.SetPositionAndRotation(_playerBoatPos.position, _playerBoatPos.rotation);

            Player.transform.parent = transform;

            isActive = true;
            _boatCam.enabled = true;

            EventManager.Instance.BoatEnter(_boatCam);

            playerLeft = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !playerLeft)
            playerLeft = true;       
    }
}
