using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENDSCENE : MonoBehaviour
{
    public GameObject[] disable;

    public AK.Wwise.Event explode;

    // Update is called once per frame
    void Update()
    {
        if (DayAndNightCycle.Instance.GetDay() == 4 && GameManager.Instance.fishCoin < 1000000)
        {
            explode.Post(gameObject);

            foreach (var item in disable)
            {
                item.SetActive(false);
            }

            enabled = false;
        }
    }
}
