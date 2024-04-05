using UnityEngine;

public class Wob : MonoBehaviour
{
    [SerializeField] private GameObject attractPoint;
    private Transform followPoint;

    private Rigidbody rb;
    private bool hasCaught = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable() 
    {
        attractPoint.SetActive(false);
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        hasCaught = false;
        followPoint = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish") && !hasCaught)
            FishCatch(other.gameObject);    
    }

    private void FishCatch(GameObject fish)
    {
        var fc = GameManager.Instance.GetFishControlScript(fish);

        if (fc.canBeFished)
        {
            hasCaught = true;

            followPoint = fish.transform;

            attractPoint.SetActive(false);

            EventManager.Instance.FishCaught(fish);
        }
        else if (fc.isAboutToDie)
        {
            //Edge case for incase wobber tries to catch a fish that has been caught
            return;
        }
        else
        {
            Debug.LogWarning($"cannot catch {fish.name}");

            EventManager.Instance.DestroyWobber();
        }
    }

    private void Update()
    {
        if (!attractPoint.activeSelf && rb.velocity.magnitude < 1f)
            attractPoint.SetActive(true);

        if (hasCaught && followPoint != null)
        {
            var velocity = followPoint.position - transform.position;

            rb.AddForce(2000 * Time.deltaTime * velocity);
        }
    }
}
