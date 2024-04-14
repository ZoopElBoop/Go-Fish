using UnityEngine;

public class ClockControl : MonoBehaviour
{
#if UNITY_EDITOR
    private void Start()
    {
        if (transform.parent == null)
        {
            Debug.LogError("Piviot needs to be parented to clock!!!");
            enabled = false;
        }
    }
#endif

    void Update()
    {
        Vector3 RotationValue = new(0, 360f * DayAndNightCycle.Instance.GetTime(), 0);
        transform.localRotation = Quaternion.Euler(RotationValue);
    }
}
