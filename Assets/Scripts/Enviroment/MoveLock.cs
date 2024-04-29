using UnityEngine;

public class MoveLock : MonoBehaviour
{
    [Range(1f, 20f)] public float startLimit;
    [Range(5f, 20f)] public float endLimit;

    private GameObject Player;

    private PlayerMovement playerMove;
    private float playerBaseSpeed;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        playerMove = PlayerScriptManager.Instance.GetScript("Movement");

        playerBaseSpeed = playerMove.moveSpeed;
    }

    void Update()
    {
        float distanceBetween = (Player.transform.position - transform.position).sqrMagnitude;

        float distanceFrom = Mathf.InverseLerp(endLimit * endLimit, startLimit * startLimit, distanceBetween);

        playerMove.moveSpeed = Mathf.Lerp(1f, playerBaseSpeed, distanceFrom);
    }

    private void OnDisable()
    {
        playerMove.moveSpeed = playerBaseSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, startLimit);
        Gizmos.DrawWireSphere(transform.position, endLimit);
    }
}
