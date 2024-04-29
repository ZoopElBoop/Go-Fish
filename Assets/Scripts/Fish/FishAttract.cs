using UnityEngine;

public class FishAttract : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Fish"))
        {
            var fc = GameManager.Instance.GetFishControlScript(other.gameObject);

            if (fc == null || !fc.canBeFished)
                return;

            fc.Attract(transform);
        }
    }
}
