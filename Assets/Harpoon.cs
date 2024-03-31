using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = 15f * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print($"hit {collision.gameObject.name}");

        rb.isKinematic = true;

        StartCoroutine(killSelf());
    }

    IEnumerator killSelf()
    {
        yield return new WaitForSeconds(20f);
        Destroy(gameObject);
    }
}
