using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wob : MonoBehaviour
{
    public GameObject attractPoint;
    public Transform followPoint;

    private Rigidbody rb;
    private bool hasCaught = false;

    private void Awake()
    {
        attractPoint.SetActive(false);
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish") && !hasCaught)
            FishCatch(other.gameObject);    
    }

    private void FishCatch(GameObject fish)
    {
        FishControl fc = fish.GetComponent<FishControl>();

        if (fc.canBeFished)
        {
            hasCaught = true;

            followPoint = fish.transform;

            EventManager.Instance.FishFished(fish);

            Destroy(attractPoint);
        }
        else
            Debug.LogWarning($"cannot catch {fish.name}");
    }

    private void Update()
    {
        if (attractPoint != null && rb.velocity.magnitude < 1f)
            attractPoint.SetActive(true);

        if (hasCaught && followPoint != null)
        {
            var velocity = followPoint.position - transform.position;
            //var dir = velocity.normalized;
            //print(dir);

            rb.AddForce(2000 * Time.deltaTime * velocity);
        }
    }
}
