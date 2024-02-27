using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    public float _Speed;
    public Transform _playerPos;
    public Camera _boatCam;

    private bool isActive = false;
    private bool playerLeft = true;
    private GameObject Player;

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (Input.GetAxis("Vertical") > 0.0f)
                transform.Translate(Vector3.forward * _Speed * Time.deltaTime);

            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0f, -0.5f, 0f, Space.Self);

            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0f, 0.5f, 0f, Space.Self);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Player.transform.parent = null;
                Player.GetComponent<CharacterController>().enabled = true;
                Player.GetComponent<PlayerMovement>().enabled = true;
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

            Player.GetComponent<CharacterController>().enabled = false;
            Player.GetComponent<PlayerMovement>().enabled = false;

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
        {
            playerLeft = true;
            //EventManager.Instance.BoatExit();
        }
    }
}
