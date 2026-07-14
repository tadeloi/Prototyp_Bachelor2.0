using UnityEngine;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour
{

    [SerializeField] public VolumeProfile[] volumeProfiles;
    [SerializeField] public Volume volume;

    public void ApplyVolumeProfile(Categories category)
    {
        volume.profile = volumeProfiles[(int)category];
    }

}