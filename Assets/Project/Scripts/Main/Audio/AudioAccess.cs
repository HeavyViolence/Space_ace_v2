using System;

namespace SpaceAce.Main.Audio
{
    public sealed record AudioAccess : IEquatable<AudioAccess>
    {
        public static AudioAccess None = new(Guid.Empty, 0f);

        public Guid ID { get; }
        public float PlaybackDuration { get; }

        public AudioAccess(Guid id, float playbackDuration)
        {
            ID = id;

            if (playbackDuration < 0f)
                throw new ArgumentOutOfRangeException(nameof(playbackDuration));

            PlaybackDuration = playbackDuration;
        }

        public bool Equals(AudioAccess other) => other is not null &&
                                                 other.ID == ID &&
                                                 other.PlaybackDuration == PlaybackDuration;

        public override int GetHashCode() => ID.GetHashCode() ^ PlaybackDuration.GetHashCode();
    }
}
