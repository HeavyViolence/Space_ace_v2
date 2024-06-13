using NaughtyAttributes;

using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ExplosionFactories;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [CreateAssetMenu(fileName = "Damage receiver config",
                     menuName = "Space ace/Configs/Damage/Damage receiver config")]
    public sealed class DamageReceiverConfig : ScriptableObject
    {
        #region explosion

        [SerializeField]
        private ExplosionSize _explosionSize = ExplosionSize.Default;

        public ExplosionSize ExplosionSize => _explosionSize;

        [SerializeField]
        private AudioCollection _explosionAudio;

        public AudioCollection ExplosionAudio => _explosionAudio;

        [SerializeField]
        private bool _shakeOnDefeat = false;

        public bool ShakeOnDefeat => _shakeOnDefeat;

        #endregion

        #region invulnerability

        [SerializeField, Space]
        private bool _invulnerable = false;

        public bool Invulnerable => _invulnerable;

        public const float MinInvulnerabilityDuration = 0f;
        public const float MaxInvulnerabilityDuration = 10f;

        [SerializeField, ShowIf("_invulnerable"), Range(MinInvulnerabilityDuration, MaxInvulnerabilityDuration)]
        private float _invulnetabilityDuration = MinInvulnerabilityDuration;

        public float InvulnerabilityDuration => _invulnetabilityDuration;

        #endregion
    }
}