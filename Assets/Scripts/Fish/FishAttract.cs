using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAttract : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FishControl>() != null)
            other.gameObject.GetComponent<FishControl>().Attract(transform);    //this is bad code
    }
}
