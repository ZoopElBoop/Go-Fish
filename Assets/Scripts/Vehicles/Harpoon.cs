using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    public int Damage = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        rb.isKinematic = false;
        rb.velocity = 15f * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sub"))
        {
            Physics.IgnoreCollision(collision.collider, col);
            return;
        }

 /*       if (collision.gameObject.CompareTag("Fish"))
        {
            FishControl fs = GameManager.Instance.GetFishControlScript(collision.gameObject);

            fs.HP -= Damage;
        }
*/
        rb.isKinematic = true;

        StartCoroutine(KillSelf());
    }

    //kill this with fire later
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fish"))
        {
            FishControl fs = GameManager.Instance.GetFishControlScript(other.gameObject);

            fs.harpoonsAttached.Add(gameObject);

            fs.HP -= Damage;

            rb.isKinematic = true;

            transform.parent = other.gameObject.transform;
        }
    }

    IEnumerator KillSelf()
    {
        yield return new WaitForSeconds(10f);
        GameManager.Instance.DestroyHarpoon(gameObject);
    }
}
