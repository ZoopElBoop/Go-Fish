using UnityEngine;

public class Boat : MonoBehaviour
{
    public float _Speed;
    public Camera _boatCam;
    public Transform _playerBoatPos;


    private bool isActive = false;
    private bool playerLeft = true;
    private GameObject Player;
    private Transform playerReturnPos;
    private Rigidbody rb;

    private CharacterController cc;
    private PlayerMovement pm;

    private void Start()
    {
        playerReturnPos = GameObject.FindGameObjectWithTag("Player Boat Exit").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
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

            cc.enabled = false;
            pm.enabled = false;

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
