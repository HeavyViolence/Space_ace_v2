using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Experience;
using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class Armor : MonoBehaviour, IArmorView, IExperienceSource
    {
        public event EventHandler<FloatValueChangedEventArgs> ValueChanged;

        [SerializeField]
        private ArmorConfig _config;

        private GamePauser _gamePauser;

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

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        private void OnEnable()
        {
            Value = _config.RandomInitialValue;
        }

        public float GetReducedDamage(float damage, float armorIgnorance)
        {
            if (Value == 0f || damage >= Value * (1f - armorIgnorance)) return damage;
            return damage * _config.GetDamageFalloffFactor(damage, Value * (1f - armorIgnorance));
        }

        public float GetExperience() => Value;
    }
}