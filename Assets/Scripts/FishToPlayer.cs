using UnityEngine;

public class FishToPlayer : MonoBehaviour
{
    [HideInInspector] public Transform Player;

    private Vector3 startingPoint;

    private float pointInTravel = 1;

    private void Awake()
    {
        enabled = false;
    }

    private void Start()
    {
        startingPoint = transform.position;
    }

    private void Update()
    {
        float distanceBetween = (Player.position - transform.position).sqrMagnitude;

        if (pointInTravel > 0.25)
            pointInTravel = Mathf.Clamp01(InverseLerp(Player.position, startingPoint, transform.position));

        if (distanceBetween > 2)
        {
            transform.Rotate(0f, 5f, 0f, Space.Self);
            transform.localScale = new Vector3(pointInTravel, pointInTravel, pointInTravel);
            transform.position = Vector3.Slerp(transform.position, Player.position, Time.deltaTime);
        }else
            EventManager.Instance.FishDisable(gameObject);
    }

    private float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}
