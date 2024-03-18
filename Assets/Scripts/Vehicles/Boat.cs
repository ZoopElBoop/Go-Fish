using UnityEngine;

public class Boat : MonoBehaviour
{
    public float _Speed;
    public Transform _playerPos;
    public Camera _boatCam;

    private bool isActive = false;
    private bool playerLeft = true;
    private GameObject Player;

    private CharacterController cc;
    private PlayerMovement pm;

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (Input.GetAxis("Vertical") > 0.0f)
                transform.Translate(_Speed * Time.deltaTime * Vector3.forward);

            transform.Rotate(0f, Input.GetAxis("Rotate") / 2, 0f, Space.Self);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.transform.parent = null;

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

            Player.transform.SetPositionAndRotation(_playerPos.position, _playerPos.rotation);

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
