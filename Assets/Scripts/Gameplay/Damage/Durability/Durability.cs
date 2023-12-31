using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public abstract class Durability : MonoBehaviour
    {
        [SerializeField]
        private DurabilityConfig _config;

        private GamePauser _gamePauser;

        protected virtual float MinInitialValue => _config.MinInitialValue;
        protected virtual float MaxInitialValue => _config.MaxInitialValue;
        protected virtual float RandomInitialValue => _config.RandomInitialValue;

        protected virtual float MinInitialValueRegen => _config.MinInitialValueRegen;
        protected virtual float MaxInitialValueRegen => _config.MaxInitialValueRegen;
        protected virtual float RandomInitialValueRegen => _config.RandomInitialValueRegen;

        public float Value { get; protected set; }
        public float MaxValue { get; protected set; }
        public float Regen { get; protected set; }

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        protected virtual void OnEnable()
        {
            MaxValue = RandomInitialValue;
            Value = MaxValue;
            Regen = RandomInitialValueRegen;
        }

        private void Update()
        {
            if (Value == 0f || _gamePauser.Paused == true) return;

            if (Regen > 0f && Value < MaxValue)
                Value += Time.deltaTime * Regen;
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f) throw new ArgumentOutOfRangeException();
            Value = Mathf.Clamp(Value - damage, 0f, MaxValue);
        }
    }
}