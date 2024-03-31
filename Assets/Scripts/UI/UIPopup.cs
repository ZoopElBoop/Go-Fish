using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public GameObject PopupCanvas;

    // Start is called before the first frame update
    void Start()
    {
        PopupCanvas.SetActive(false);
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
