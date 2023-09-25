using UnityEngine;
using UnityEngine.Audio;

namespace SpaceAce.Main.Audio
{
    public enum AudioClipPriority
    {
        Highest, High, Medium, Low, Lowest
    }

    public sealed class AudioProperties
    {
        public AudioClip Clip { get; }
        public AudioMixerGroup OutputAudioGroup { get; }
        public float Volume { get; }
        public AudioClipPriority Priority { get; }
        public float SpatialBlend { get; }
        public float Pitch { get; }
        public bool Loop { get; }
        public Transform AudioSourceAnchor { get; }
        public Vector3 PlayPosition { get; }

        public AudioProperties(AudioClip clip,
                               AudioMixerGroup group,
                               float volume,
                               AudioClipPriority priority,
                               float spatialBlend,
                               float pitch,
                               bool loop,
                               Transform audioSourceAnchor,
                               Vector3 playPosition)
        {
            Clip = clip;
            OutputAudioGroup = group;
            Priority = priority;
            SpatialBlend = Mathf.Clamp01(spatialBlend);
            Pitch = Mathf.Clamp(pitch, 0f, 2f);
            PlayPosition = playPosition;
            Loop = loop;
            AudioSourceAnchor = audioSourceAnchor;
            Volume = Mathf.Clamp01(volume);
        }
    }
}