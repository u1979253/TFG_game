using UnityEngine;

[CreateAssetMenu(fileName = "SFX", menuName = "Audio/SFX")]
public class AudioSFX : SoundSO
{
    public AudioType.SFX TypeName;
    public AudioClip[] clips;
}
