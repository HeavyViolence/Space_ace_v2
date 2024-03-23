using UnityEngine;
using UnityEngine.Audio;

namespace SpaceAce.Main.Audio
{
    public readonly struct AudioProperties
    {
        public AudioClip Clip { get; }
        public AudioMixerGroup OutputAudioGroup { get; }
        public float Volume { get; }
        public AudioPriority Priority { get; }
        public float SpatialBlend { get; }
        public float Pitch { get; }

        public AudioProperties(AudioClip clip,
                               AudioMixerGroup group,
                               float volume,
                               AudioPriority priority,
                               float spatialBlend,
                               float pitch)
        {
            Clip = clip;
            OutputAudioGroup = group;
            Volume = Mathf.Clamp01(volume);
            Priority = priority;
            SpatialBlend = Mathf.Clamp01(spatialBlend);
            Pitch = Mathf.Clamp(pitch, 0f, 2f);
        }
    }
}