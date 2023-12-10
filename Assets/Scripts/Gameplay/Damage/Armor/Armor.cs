using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public abstract class Armor : MonoBehaviour
    {
        [SerializeField]
        private ArmorConfig _config;

        protected virtual float MinInitialValue => _config.MinInitialValue;
        protected virtual float MaxInitialValue => _config.MaxInitialValue;
        protected virtual float RandomInitialValue => _config.RandomInitialValue;

        public float Value { get; private set; }

        protected virtual void OnEnable()
        {
            Value = RandomInitialValue;
        }

        public float GetDamageToBeDealt(float damageReceived)
        {
            if (damageReceived < 0f)
                throw new ArgumentOutOfRangeException(nameof(damageReceived),
                    $"Incoming damage must not be negative!");

            return damageReceived * damageReceived / Value;
        }
    }
}