using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class SelectionInterface : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text Name;
    public TMP_Text Count;
    public Image Image;
    public GameObject selectedUI;

    public int ID;
    [HideInInspector] public Interactable interact;

    private void Awake()
    {
        interact = GetComponent<Interactable>();
        selectedUI.SetActive(false);
    }

    public void Selected()
    {
        selectedUI.SetActive(true);
    }

    public void Deselected()
    {
        selectedUI.SetActive(false);
    }

    public void IsActive(bool status) 
    {
        interact.CanInteract(status);
    }
}