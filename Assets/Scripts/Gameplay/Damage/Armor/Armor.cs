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

        public float Value { get; protected set; }

        protected virtual void OnEnable()
        {
            Value = RandomInitialValue;
        }

        public float GetReducedDamage(float damage)
        {
            if (damage < 0f)
                throw new ArgumentOutOfRangeException(nameof(damage),
                    $"Incoming damage must not be negative!");

            return damage * damage / Value;
        }

        public void ApplyDamage(float damage)
        {
            if (Value <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage),
                    "Damage value must be positive!");

            Value = Mathf.Clamp(Value - damage, 0f, float.MaxValue);
        }
    }
}