using UnityEngine;

public class SwitchControl : MonoBehaviour
{
    public void Rotate() 
    {
        Vector3 newAngle = new(
            -90f,
            transform.eulerAngles.y + 180f,
            0f);

        var a = Quaternion.Euler(newAngle);

        transform.rotation = a;

        print("rotating");
    }
}
