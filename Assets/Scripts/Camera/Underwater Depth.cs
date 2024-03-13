using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterDepth : MonoBehaviour
{
    [Header("Camera Parameters")]
    [SerializeField] private Transform Camera;
    [SerializeField] private float Depth = 0;

    [Header("Post Processing Volume")]
    [SerializeField] private Volume postProcessingVolume;

    [Header("Post Processing Profiles")]
    [SerializeField] private VolumeProfile surfaceProfile;
    [SerializeField] private VolumeProfile underwaterProfile;

    void Update()
    {
        if (transform.position.y < Depth)
            EnableFx(true);
        else
            EnableFx(false);
    }

    private void EnableFx(bool Active)
    {
        if (Active)
        {
            postProcessingVolume.profile = underwaterProfile;
        }
        else
        {
            postProcessingVolume.profile= surfaceProfile;
        }
    }
}
