using UnityEngine;

public class NPCAnims : MonoBehaviour
{
    public Animator anim;

    private void Start()
    {
        //anim.SetTrigger("Player Far");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.ResetTrigger("Player Far");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetTrigger("Player Far");
        }
    }
}
