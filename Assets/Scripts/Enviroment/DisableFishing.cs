using UnityEngine;

public class DisableFishing : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.InVessel == false)
        { 
            GameManager.Instance.CanFish(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !GameManager.Instance.cutSceneOverride)
            GameManager.Instance.CanFish(true);
    }
}
