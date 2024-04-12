using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public Transform Pivot;

    [Range(1.0f, 10.0f)] public float doorSpeed = 10.0f;
    public Vector3 doorOpenLimit;

    private bool open = false;

    private Quaternion rotationEnd;
    private Quaternion rotationStart;

    private void Start()
    {
        rotationStart = Pivot.rotation;
        rotationEnd = Pivot.rotation;
    }

    private void Update()
    {
        Pivot.rotation = Quaternion.Slerp(Pivot.rotation, rotationEnd, doorSpeed * Time.deltaTime);
    }

    public void MoveDoor()
    {
        if (!open)
        {
            Vector3 RotationValue = new Vector3(doorOpenLimit.x, doorOpenLimit.y, doorOpenLimit.z) + rotationStart.eulerAngles;
            rotationEnd = Quaternion.Euler(RotationValue);

            open = true;
        }
        else
        {
            rotationEnd = rotationStart;

            open = false;
        }
    }

}