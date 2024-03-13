using UnityEngine;

public class FishAttract : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Fish"))    
            other.gameObject.transform.root.GetComponent<FishControl>().Attract(transform);
    }
}
