using UnityEngine;

namespace SpaceAce.Main.Audio
{
    public sealed class MusicPlayerSettings
    {
        public const float MinPlaybackDelay = 0f;
        public const float MaxPlaybackDelay = 300f;
        public const float DefaultPlaybackDelay = 5f;

        public static MusicPlayerSettings Default => new(DefaultPlaybackDelay, DefaultPlaybackDelay);

        public float PlaybackStartDelay { get; }
        public float PlaybackDelay { get; }

        public MusicPlayerSettings(float playbackStartDelay, float playbackDelay)
        {
            PlaybackStartDelay = Mathf.Clamp(playbackStartDelay, MinPlaybackDelay, MaxPlaybackDelay);
            PlaybackDelay = Mathf.Clamp(playbackDelay, MinPlaybackDelay, MaxPlaybackDelay);
        }
    }
}