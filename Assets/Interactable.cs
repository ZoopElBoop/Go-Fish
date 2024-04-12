using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Interactable : MonoBehaviour
{
    public UnityEvent interactEvent;

    [Header ("Interaction Setting")]
    private bool interactEnabled = true;
    [Min (0f)] public float timeInSecondsBeforeReActive;

    private bool timeOver = true;

    [Header("Audio Setting")]
    public AK.Wwise.Event interact;
    //public AK.Wwise.Event activeAgain;

    public void CanInteract(bool status)
    {
        interactEnabled = status;

        if (!status)
            gameObject.layer = 0;
        else
            gameObject.layer = LayerMask.NameToLayer("Interact"); 
    }

    public void ActivateEvent()
    {
        if (interactEnabled && timeOver)
        {
            if (!interact.IsUnityNull())
                interact.Post(gameObject);

            interactEvent.Invoke();
            StartCoroutine(DelayToActive());
        }
    }

    IEnumerator DelayToActive() 
    {
        timeOver = false;
        yield return new WaitForSeconds(timeInSecondsBeforeReActive);
        timeOver = true;
    }
}
