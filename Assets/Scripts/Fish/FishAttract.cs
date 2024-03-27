using UnityEngine;

public class FishAttract : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Fish"))
        {
            var fc = GameManager.Instance.GetFishConrolScript(other.gameObject);

            if (fc == null)
                return;

            fc.Attract(transform);
        }
    }
}
