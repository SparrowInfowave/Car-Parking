using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "AudioClipInfo",fileName = "NewAudioClipInfo")]

    public class AudioClipInfo : ScriptableObject
    { 
        public string id = "";
        public AudioClip audioClip = null;
        public SoundManager.SoundType type = SoundManager.SoundType.SoundEffect;
        public bool playAndLoopOnStart = false;
        [Range(0, 1)] public float clipVolume = 1;
    }
}
