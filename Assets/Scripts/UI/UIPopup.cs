using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public GameObject PopupCanvas;
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        PopupCanvas.SetActive(false);
    }

    private void Update()
    {
        if (cam != null && PopupCanvas.activeSelf)
            PopupCanvas.transform.rotation = Quaternion.LookRotation(PopupCanvas.transform.position - cam.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PopupCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PopupCanvas.SetActive(false);
    }
}
