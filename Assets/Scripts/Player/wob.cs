using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wob : MonoBehaviour
{
    public GameObject attractPoint;

    private Rigidbody rb;
    private bool hasCaught = false;

    private void Awake()
    {
        attractPoint.SetActive(false);
        Debug.Break();
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish") && !hasCaught)
            FishCatch(other.gameObject);    //this is bad code
    }

/*    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 4)
            attractPoint.SetActive(true);
    }*/

    private void FishCatch(GameObject fish)
    {
        FishControl fc = fish.GetComponent<FishControl>();

        if (fc.canBeFished)
        {
            hasCaught = true;

            EventManager.Instance.FishFished(fish);

            Destroy(attractPoint);
            enabled = false;
        }
        else
            Debug.LogWarning($"cannot catch {fish.name}");
    }

    private void Update()
    {
        if (rb.velocity.magnitude < 1f)
            attractPoint.SetActive(true);
    }
}
