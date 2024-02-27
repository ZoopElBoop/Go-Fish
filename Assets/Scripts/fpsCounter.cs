using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class fpsCounter : MonoBehaviour
{
    public TMP_Text fps;
    private float delay;

    void Update()
    {
        delay += Time.deltaTime;

        if (delay > 0.75)
        {
            delay = 0;
            fps.text = (int)(1f / Time.unscaledDeltaTime) + " FPS";
        }
    }
}
