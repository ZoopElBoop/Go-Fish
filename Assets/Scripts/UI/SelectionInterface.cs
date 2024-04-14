using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class SelectionInterface : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text Name;
    public TMP_Text Count;
    public Image Image;

    [HideInInspector] public int ID;
    [HideInInspector] public Interactable interact;

    private void Awake()
    {
        interact = GetComponent<Interactable>();
    }

    public void Selected()
    {
        //change background to show selected
    }

    public void Deselected()
    {
        //change background to show deselected
    }

    public void IsActive(bool status) 
    {
        interact.CanInteract(status);
    }
}