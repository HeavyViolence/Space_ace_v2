using Cysharp.Threading.Tasks;

using SpaceAce.Main;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Shooting
{
    public abstract class Shooting : MonoBehaviour
    {
        public event EventHandler Overheated, CooledDown;
        public event EventHandler ShootingStarted, ShootingStopped;

        [SerializeField]
        private ShootingConfig _config;

        private AnimationCurve HeatLossFactorCurve => _config.HeatLossFactorCurve;

        private float _baseHeatLossRate;

        protected GamePauser GamePauser;

        public virtual float MinInitialHeatCapacity => _config.MinInitialHeatCapacity;
        public virtual float MaxInitialHeatCapacity => _config.MaxInitialHeatCapacity;
        public virtual float RandomInitialHeatCapacity => _config.RandomInitialHeatCapacity;

        public virtual float MinInitialBaseHeatLossRate => _config.MinInitialBaseHeatLossRate;
        public virtual float MaxInitialBaseHeatLossRate => _config.MaxInitialBaseHeatLossRate;
        public virtual float RandomInitialBaseHeatLossRate => _config.RandomInitialBaseHeatLossRate;

        public virtual float MinInitialOverheatDuration => _config.MinInitialOverheatDuration;
        public virtual float MaxInitialOverheatDuration => _config.MaxInitialOverheatDuration;
        public virtual float RandomInitialOverheatDuration => _config.RandomInitialOverheatDuration;

        public float CurrentHeat { get; protected set; }
        public float HeatCapacity { get; protected set; }
        public float CurrentNormalizedHeat => CurrentHeat / HeatCapacity;
        public float OverheatDuration { get; protected set; }

        public bool Overheat { get; protected set; }
        public bool Firing { get; protected set; }

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            GamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        protected virtual void OnEnable()
        {
            CurrentHeat = 0f;
            HeatCapacity = RandomInitialHeatCapacity;
            _baseHeatLossRate = RandomInitialBaseHeatLossRate;
            OverheatDuration = RandomInitialOverheatDuration;
            Overheat = false;
            Firing = false;
        }

        protected virtual void Update()
        {
            if (GamePauser.Paused == true || CurrentHeat <= 0f || Overheat == true) return;

            CurrentHeat -= GetCurrentHeatLossRate();
            CurrentHeat = Mathf.Clamp(CurrentHeat, 0f, HeatCapacity);
        }

        protected virtual float GetCurrentHeatLossRate()
        {
            float factor = HeatLossFactorCurve.Evaluate(CurrentNormalizedHeat);
            return _baseHeatLossRate * factor * Time.deltaTime;
        }

        protected async UniTask PerformOverheatAsync()
        {
            if (Overheat == true) return;

            Overheat = true;
            Overheated?.Invoke(this, EventArgs.Empty);

            float timer = 0f;

            while (timer < OverheatDuration)
            {
                timer += Time.deltaTime;

                while (GamePauser.Paused == true) await UniTask.Yield();

                await UniTask.Yield();
            }

            Overheat = false;
            CooledDown?.Invoke(this, EventArgs.Empty);
        }

        protected virtual async UniTask FireAsync(CancellationToken token = default)
        {
            if (Firing == true)
            {
                await UniTask.Yield();
                return;
            }

            Firing = true;
            ShootingStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Ceasefire()
        {
            if (Firing == false) return;

            Firing = false;
            ShootingStopped?.Invoke(this, EventArgs.Empty);
        }
    }
}