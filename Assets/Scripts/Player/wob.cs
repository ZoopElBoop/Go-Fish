using UnityEngine;

public class Wob : MonoBehaviour
{
    public GameObject attractPoint;
    public Transform followPoint;

    private Rigidbody rb;
    private bool hasCaught = false;

    private void Awake()
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
        FishControl fc = fish.GetComponent<FishControl>();

        if (fc.canBeFished)
        {
            hasCaught = true;

            followPoint = fish.transform;

            attractPoint.SetActive(false);

            EventManager.Instance.FishFished(fish);
        }
        else
        {
            Debug.LogWarning($"cannot catch {fish.name}");

            //Add destroy wobber event for fishing script 
        }
    }

    private void Update()
    {
        if (!attractPoint.activeSelf && rb.velocity.magnitude < 1f)
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
