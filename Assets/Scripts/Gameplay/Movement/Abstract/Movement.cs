using Cysharp.Threading.Tasks;

using SpaceAce.Architecture;
using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main;

using System;
using System.Threading;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageDealer))]
    public abstract class Movement : MonoBehaviour, IEscapable, IMovementBehaviourSupplier
    {
        protected static readonly CachedService<MasterCameraHolder> MasterCameraHolder = new();
        protected static readonly CachedService<MasterCameraShaker> MasterCameraShaker = new();
        protected static readonly CachedService<GamePauser> GamePauser = new();

        public event EventHandler Escaped;

        [SerializeField] private MovementConfig _config;

        private DamageDealer _collisionDamageDealer;

        private MovementBehaviour _behaviour;
        private MovementData _data;

        protected Rigidbody2D Body { get; private set; }

        protected virtual float MinHorizontalSpeed => _config.MinHorizontalSpeed;
        protected virtual float MaxHorizontalSpeed => _config.MaxHorizontalSpeed;
        protected virtual float NextHorizontalSpeed => AuxMath.GetRandom(MinHorizontalSpeed, MaxHorizontalSpeed);

        protected virtual float MinVerticalSpeed => _config.MinVerticalSpeed;
        protected virtual float MaxVerticalSpeed => _config.MaxVerticalSpeed;
        protected virtual float NextVerticalSpeed => AuxMath.GetRandom(MinVerticalSpeed, MaxVerticalSpeed);

        protected virtual float MinSpatialSpeed => _config.MinSpatialSpeed;
        protected virtual float MaxSpatialSpeed => _config.MaxSpatialSpeed;
        protected virtual float NextSpatialSpeed => AuxMath.GetRandom(MinSpatialSpeed, MaxSpatialSpeed);

        protected virtual float MinRotationSpeed => _config.LowestRotationSpeed;
        protected virtual float MaxRotationSpeed => _config.HighestRotationSpeed;
        protected virtual float NextRotationSpeed => AuxMath.GetRandom(MinRotationSpeed, MaxRotationSpeed);
        protected bool RotationEnabled => MinRotationSpeed != 0f || MaxRotationSpeed != 0f;

        protected virtual float UpperBound => _config.UpperBound;
        protected virtual float LowerBound => _config.LowerBound;
        protected virtual float LeftBound => _config.LeftBound;
        protected virtual float RightBound => _config.RightBound;

        protected virtual float MinCollisionDamage => _config.LowestCollisionDamage;
        protected virtual float MaxCollisionDamage => _config.HighestCollisionDamage;
        protected virtual float NextCollisionDamage => AuxMath.GetRandom(MinCollisionDamage, MaxCollisionDamage);
        protected bool CollisionDamageEnabled => MaxCollisionDamage > 0f;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            _collisionDamageDealer = GetComponent<DamageDealer>();
        }

        protected virtual void OnEnable()
        {
            _collisionDamageDealer.Hit += CollisionHitEventHandler;
        }

        protected virtual void OnDisable()
        {
            _collisionDamageDealer.Hit -= CollisionHitEventHandler;
        }

        protected virtual void FixedUpdate()
        {
            if (_behaviour is null ||
                _data is null ||
                GamePauser.Access.Paused == true) return;

            _behaviour(Body, ref _data);
        }

        #region interfaces

        public async UniTask WatchForEscapeAsync(Func<bool> condition, CancellationToken token)
        {
            while (MasterCameraHolder.Access.InsideViewprot(transform.position) == false)
            {
                if (token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            while (MasterCameraHolder.Access.InsideViewprot(transform.position) == true)
            {
                if (token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            while (condition() == false)
            {
                if (token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            Escaped?.Invoke(this, EventArgs.Empty);
        }

        public void SupplyMovementBehaviour(MovementBehaviour behaviour, MovementData data)
        {
            _behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour),
                $"Attempted to pass an empty {typeof(MovementBehaviour)}!");

            _data = data ?? throw new ArgumentNullException(nameof(data),
                $"Attempted to pass an empty {typeof(MovementData)}!");
        }

        #endregion

        #region event handlers

        private void CollisionHitEventHandler(object sender, HitEventArgs e)
        {
            e.DamageReceiver.ApplyDamage(NextCollisionDamage);
            _config.CollisionAudio.PlayRandomAudioClip(e.HitPosition);

            if (_config.CameraShakeOnCollisionEnabled == true)
                MasterCameraShaker.Access.ShakeOnCollision();
        }

        #endregion
    }
}