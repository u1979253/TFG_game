using UnityEngine;

[CreateAssetMenu(fileName = "Loop", menuName = "Audio/Loop")]
public class AudioLoop : SoundSO
{
    public AudioType.Loop TypeName;
    public AudioClip clip;
}
