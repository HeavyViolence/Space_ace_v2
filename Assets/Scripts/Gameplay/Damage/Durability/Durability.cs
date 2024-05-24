using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Experience;
using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class Durability : MonoBehaviour, IDurabilityView, IExperienceSource
    {
        public event EventHandler<FloatValueChangedEventArgs> ValueChanged, MaxValueChanged, RegenChanged;

        [SerializeField]
        private DurabilityConfig _config;

        private GamePauser _gamePauser;

        public Sprite Icon => _config.Icon;

        private float _value;
        public float Value
        {
            get => _value;

            set
            {
                ValueChanged?.Invoke(this, new(_value, value));
                _value = value;
            }
        }

        private float _maxValue;
        public float MaxValue
        {
            get => _maxValue;

            set
            {
                MaxValueChanged?.Invoke(this, new(_maxValue, value));
                _maxValue = value;
            }
        }

        private float _regen;
        public float Regen
        {
            get => _regen;

            set
            {
                RegenChanged?.Invoke(this, new(_regen, value));
                _regen = value;
            }
        }

        private float _regainedValue;

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        private void OnEnable()
        {
            MaxValue = _config.RandomInitialValue;
            Value = MaxValue;
            Regen = _config.RandomInitialValueRegen;
            _regainedValue = 0f;
        }

        private void Update()
        {
            if (Value == 0f || _gamePauser.Paused == true) return;

            if (Regen > 0f && Value < MaxValue)
            {
                float valueGainThisFrame = Time.deltaTime * Regen;

                Value += valueGainThisFrame;
                _regainedValue += valueGainThisFrame;
            }
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f) throw new ArgumentOutOfRangeException();
            Value = Mathf.Clamp(Value - damage, 0f, MaxValue);
        }

        public float GetExperience() => MaxValue + _regainedValue;
    }
}