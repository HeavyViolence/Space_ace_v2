using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Experience;

using System;

using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class Armor : MonoBehaviour, IArmorView, IExperienceSource
    {
        public event EventHandler<FloatValueChangedEventArgs> ValueChanged;

        [SerializeField]
        private ArmorConfig _config;

        private float _value;
        public float Value
        {
            get => _value;

            set
            {
                float oldValue = _value;
                float newValue = value;

                _value = newValue;
                ValueChanged?.Invoke(this, new(oldValue, newValue));
            }
        }

        private void OnEnable()
        {
            Value = _config.RandomInitialValue;
        }

        public float GetReducedDamage(float damage, float armorIgnoring)
        {
            if (Value == 0f || damage >= Value * (1f - armorIgnoring)) return damage;
            return damage * _config.GetDamageFalloffFactor(damage, Value * (1f - armorIgnoring));
        }

        public float GetExperience() => Value;
    }
}