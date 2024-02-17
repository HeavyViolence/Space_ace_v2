using SpaceAce.Main;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public sealed class Durability : MonoBehaviour
    {
        [SerializeField]
        private DurabilityConfig _config;

        private GamePauser _gamePauser;

        public float Value { get; private set; }
        public float MaxValue { get; private set; }
        public float Regen { get; private set; }

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