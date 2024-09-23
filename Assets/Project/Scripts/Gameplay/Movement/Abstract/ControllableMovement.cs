using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Experience;

using UnityEngine;

namespace SpaceAce.Gameplay.Movement
{
    [RequireComponent(typeof(DamageDealer))]
    public abstract class ControllableMovement : Movement, IExperienceSource
    {
        [SerializeField] private MovementConfig _config;

        private DamageDealer _collisionDamageDealer;

        protected virtual float MinHorizontalSpeed => _config.MinHorizontalSpeed;
        protected virtual float MaxHorizontalSpeed => _config.MaxHorizontalSpeed;
        protected virtual float NextHorizontalSpeed => _config.RandomHorizontalSpeed;

        protected virtual float MinVerticalSpeed => _config.MinVerticalSpeed;
        protected virtual float MaxVerticalSpeed => _config.MaxVerticalSpeed;
        protected virtual float NextVerticalSpeed => _config.RandomVerticalSpeed;

        protected virtual float MinMovementStiffness => _config.MovementStiffness.MinMovementStiffness;
        protected virtual float MaxMovementStiffness => _config.MovementStiffness.MaxMovementStiffness;
        protected virtual float NextMovementStiffness => _config.MovementStiffness.RandomMovementStiffness;

        protected virtual float MinBrakingSmoothness => _config.MovementStiffness.MinBrakingStiffness;
        protected virtual float MaxBrakingSmoothness => _config.MovementStiffness.MaxBrakingStiffness;
        protected virtual float NextBrakingSmoothness => _config.MovementStiffness.RandomBrakingStiffness;

        protected virtual float MinViewportReboundStiffness => _config.MovementStiffness.MinViewportReboundStiffness;
        protected virtual float MaxViewportReboundStiffness => _config.MovementStiffness.MaxViewportReboundStiffness;
        protected virtual float NextViewportReboundStiffness => _config.MovementStiffness.RandomViewportReboundStiffness;

        protected virtual float MinRotationStiffness => _config.MovementStiffness.MinRotationStiffness;
        protected virtual float MaxRotationStiffness => _config.MovementStiffness.MaxRotationStiffness;
        protected virtual float NextRotationStiffness => _config.MovementStiffness.RandomRotationStiffness;

        protected virtual float ModifiedLowerBound => MasterCameraHolder.ViewportLowerBound * _config.BoundsDisplacement.LowerBoundDisplacement;
        protected virtual float ModifiedUpperBound => MasterCameraHolder.ViewportUpperBound * _config.BoundsDisplacement.UpperBoundDisplacement;
        protected virtual float ModifiedRightBound => MasterCameraHolder.ViewportRightBound * _config.BoundsDisplacement.SideBoundsDisplacement;
        protected virtual float ModifiedLeftBound => MasterCameraHolder.ViewportLeftBound * _config.BoundsDisplacement.SideBoundsDisplacement;

        protected virtual float MinCollisionDamage => _config.LowestCollisionDamage;
        protected virtual float MaxCollisionDamage => _config.HighestCollisionDamage;
        protected virtual float NextCollisionDamage => _config.RandomCollisionDamage;
        protected bool CollisionDamageEnabled => MaxCollisionDamage > 0f;

        protected override void Awake()
        {
            base.Awake();

            if (TryGetComponent(out DamageDealer damageDealer) == true) _collisionDamageDealer = damageDealer;
            else throw new MissingComponentException(nameof(DamageDealer));
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _collisionDamageDealer.Hit += CollisionHitEventHandler;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _collisionDamageDealer.Hit -= CollisionHitEventHandler;
        }

        private void CollisionHitEventHandler(object sender, HitEventArgs e)
        {
            e.Damageable.ApplyDamage(NextCollisionDamage);
            AudioPlayer.PlayOnceAsync(_config.CollisionAudio.Random, e.HitPosition, null, true).Forget();
            MasterCameraShaker.ShakeOnCollision();
        }

        public float GetExperience() => MaxHorizontalSpeed + MaxVerticalSpeed;
    }
}