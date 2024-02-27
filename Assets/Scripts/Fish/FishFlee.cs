using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlee : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    { 
        if (other.gameObject.GetComponent<FishControl>() != null)
            other.gameObject.GetComponent<FishControl>().Flee(transform);    //this is bad code
    }
}
