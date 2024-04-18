using TMPro;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public bool followPlayer;
    public TMP_Text PopupUI;
    private Camera cam;


    private void Start()
    {
        cam = Camera.main;

        PopupUI.enabled = false;
    }

    private void Update()
    {
        if (followPlayer && cam != null)
            PopupUI.transform.rotation = Quaternion.LookRotation(PopupUI.transform.position - cam.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PopupUI.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            PopupUI.enabled = false;
    }
}
