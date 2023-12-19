using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Damage;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [RequireComponent(typeof(DamageDealer))]
    public abstract class ControllableMovement : Movement
    {
        [SerializeField] private MovementConfig _config;

        private DamageDealer _collisionDamageDealer;

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

        protected virtual float UpperBound => MasterCameraHolder.GetViewportUpperBoundWithOffset(_config.UpperBoundDisplacement);
        protected virtual float LowerBound => MasterCameraHolder.GetViewportLowerBoundWithOffset(_config.LowerBoundDisplacement);
        protected virtual float LeftBound => MasterCameraHolder.GetViewportLeftBoundWithOffset(_config.LeftBoundDisplacement);
        protected virtual float RightBound => MasterCameraHolder.GetViewportRightBoundWithOffset(_config.RightBoundDisplacement);

        protected virtual float MinCollisionDamage => _config.LowestCollisionDamage;
        protected virtual float MaxCollisionDamage => _config.HighestCollisionDamage;
        protected virtual float NextCollisionDamage => AuxMath.GetRandom(MinCollisionDamage, MaxCollisionDamage);
        protected bool CollisionDamageEnabled => MaxCollisionDamage > 0f;

        protected override void Awake()
        {
            base.Awake();

            if (TryGetComponent(out DamageDealer damageDealer) == true) _collisionDamageDealer = damageDealer;
            else throw new MissingComponentException(nameof(DamageDealer));
        }

        protected virtual void OnEnable()
        {
            _collisionDamageDealer.Hit += CollisionHitEventHandler;
        }

        protected virtual void OnDisable()
        {
            _collisionDamageDealer.Hit -= CollisionHitEventHandler;
        }

        private void CollisionHitEventHandler(object sender, HitEventArgs e)
        {
            e.DamageReceiver.ApplyDamage(NextCollisionDamage);
            AudioPlayer.PlayOnceAsync(_config.CollisionAudio.Random, e.HitPosition, null, default, true).Forget();
            MasterCameraShaker.ShakeOnCollisionAsync().Forget();
        }
    }
}