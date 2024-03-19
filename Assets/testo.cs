using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testo : MonoBehaviour
{
    [SerializeField] private List<KeyCode> Inputs = new();

    private void Update()
    {
        for (int i = 0; i < Inputs.Count; i++)
        {
             if (Input.GetKeyDown(Inputs[i]))
                print("you pressed" + Inputs[i].ToString());

        }
    }
}
