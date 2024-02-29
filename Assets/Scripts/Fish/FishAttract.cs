using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAttract : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish"))
            other.gameObject.GetComponent<FishControl>().Attract(transform);
    }
}
