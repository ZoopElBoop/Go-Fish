using UnityEngine;

public class ClockControl : MonoBehaviour
{

    void Update()
    {
        Vector3 RotationValue = new(0,0, 360 - (360f * DayAndNightCycle.Instance.GetTime()));
        transform.localRotation = Quaternion.Euler(RotationValue);
    }
}
