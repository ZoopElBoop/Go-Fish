using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wob : MonoBehaviour
{
    public GameObject attractPoint;

    private bool hasCaught = false;

    private void Awake()
    {
        attractPoint.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish") && !hasCaught)
            FishCatch(other.gameObject);    //this is bad code
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 4)
            attractPoint.SetActive(true);
    }

    private void FishCatch(GameObject fish)
    {
        FishControl fc = fish.GetComponent<FishControl>();

        if (fc.canBeFished)
        {
            hasCaught = true;

            EventManager.Instance.FishFished(fish);

            Destroy(attractPoint);
            Destroy(this);
        }
        else
            Debug.LogWarning($"cannot catch {fish.name}");
    }
}
