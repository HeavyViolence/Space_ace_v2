using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ExplosionFactories;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [CreateAssetMenu(fileName = "Damage receiver config",
                     menuName = "Space ace/Configs/Damage/Damage receiver config")]
    public sealed class DamageReceiverConfig : ScriptableObject
    {
        [SerializeField]
        private ExplosionSize _explosionSize = ExplosionSize.Default;

        [SerializeField]
        private AudioCollection _explosionAudio;

        [SerializeField]
        private bool _shakeOnDefeat = false;

        [SerializeField]
        private bool _indestructible = false;

        public ExplosionSize ExplosionSize => _explosionSize;

        public AudioCollection ExplosionAudio => _explosionAudio;

        public bool ShakeOnDefeat => _shakeOnDefeat;

        public bool Indestructible => _indestructible;
    }
}