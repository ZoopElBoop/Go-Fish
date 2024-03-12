using UnityEngine;

public class FishToPlayer : MonoBehaviour
{
    public Transform Player;

    private Vector3 startingPoint;
    private Vector3 startingScale;

    private void Awake()
    {
        enabled = false;
    }

    private void Start()
    {
        startingPoint = transform.position;
        startingScale = transform.localScale;
    }

    private void Update()
    {
        float distanceBetween = (Player.position - transform.position).sqrMagnitude;

        float pointInTravel = Mathf.Clamp01(InverseLerp(Player.position, startingPoint, transform.position));

        if (pointInTravel < 0.25)
            pointInTravel = 0.25f;

        if (distanceBetween > 2)
        {
            print(pointInTravel);
            transform.Rotate(0f, 5f, 0f, Space.Self);
            transform.localScale = new Vector3(pointInTravel, pointInTravel, pointInTravel);
            transform.position = Vector3.Slerp(transform.position, Player.position, Time.deltaTime);
        }else
            EventManager.Instance.FishDisable(gameObject);
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}
