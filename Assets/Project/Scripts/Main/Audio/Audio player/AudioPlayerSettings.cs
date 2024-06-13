using UnityEngine;

namespace SpaceAce.Main.Audio
{
    public sealed class AudioPlayerSettings
    {
        public const float MinVolume = -80f;
        public const float MaxVolume = 20f;
        public const float DefaultVolume = 0f;

        public static AudioPlayerSettings Default => new(DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume,
                                                         DefaultVolume);

        public float MasterVolume { get; }
        public float ShootingVolume { get; }
        public float ExplosionsVolume { get; }
        public float BackgroundVolume { get; }
        public float InterfaceVolume { get; }
        public float MusicVolume { get; }
        public float InteractionsVolume { get; }
        public float NotificationsVolume { get; }

        public AudioPlayerSettings(float masterVolume,
                                   float shootingVolume,
                                   float explosionsVolume,
                                   float backgroundVolume,
                                   float interfaceVolume,
                                   float musicVolume,
                                   float interactionsVolume,
                                   float notificationsVolume)
        {
            MasterVolume = Mathf.Clamp(masterVolume, MinVolume, MaxVolume);
            ShootingVolume = Mathf.Clamp(shootingVolume, MinVolume, MaxVolume);
            ExplosionsVolume = Mathf.Clamp(explosionsVolume, MinVolume, MaxVolume);
            BackgroundVolume = Mathf.Clamp(backgroundVolume, MinVolume, MaxVolume);
            InterfaceVolume = Mathf.Clamp(interfaceVolume, MinVolume, MaxVolume);
            MusicVolume = Mathf.Clamp(musicVolume, MinVolume, MaxVolume);
            InteractionsVolume = Mathf.Clamp(interactionsVolume, MinVolume, MaxVolume);
            NotificationsVolume = Mathf.Clamp(notificationsVolume, MinVolume, MaxVolume);
        }
    }
}