using UnityEngine;

public class FishFlee : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root.CompareTag("Fish"))
        {
            var fc = GameManager.Instance.GetFishControlScript(other.gameObject);

            if (fc == null)
                return;

            fc.Flee(transform);
        }
    }
}
