using UnityEngine;

namespace CreativeMode
{
    public class AudioAsset
    {
        public AudioMetadata Meta { get; }
        public AudioClip Clip { get; }

        public AudioAsset(AudioClip clip, AudioMetadata meta)
        {
            Clip = clip;
            Meta = meta;
        }
    }
}