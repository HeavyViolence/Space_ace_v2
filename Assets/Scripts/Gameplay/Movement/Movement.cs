using Cysharp.Threading.Tasks;

using SpaceAce.Auxiliary;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageDealer))]
    public abstract class Movement : MonoBehaviour, IEscapable, IMovementBehaviourSupplier
    {
        public event EventHandler Escaped;

        [SerializeField] private MovementConfig _config;

        protected GamePauser GamePauser;
        protected AudioPlayer AudioPlayer;
        protected MasterCameraHolder MasterCameraHolder;
        protected MasterCameraShaker MasterCameraShaker;

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

        protected virtual float UpperBound => MasterCameraHolder.GetViewportUpperBoundWithOffset(_config.UpperBoundDisplacement);
        protected virtual float LowerBound => MasterCameraHolder.GetViewportLowerBoundWithOffset(_config.LowerBoundDisplacement);
        protected virtual float LeftBound => MasterCameraHolder.GetViewportLeftBoundWithOffset(_config.LeftBoundDisplacement);
        protected virtual float RightBound => MasterCameraHolder.GetViewportRightBoundWithOffset(_config.RightBoundDisplacement);

        protected virtual float MinCollisionDamage => _config.LowestCollisionDamage;
        protected virtual float MaxCollisionDamage => _config.HighestCollisionDamage;
        protected virtual float NextCollisionDamage => AuxMath.GetRandom(MinCollisionDamage, MaxCollisionDamage);
        protected bool CollisionDamageEnabled => MaxCollisionDamage > 0f;

        [Inject]
        private void Construct(GamePauser gamePauser,
                               AudioPlayer audioPlayer,
                               MasterCameraHolder masterCameraHolder,
                               MasterCameraShaker masterCameraShaker)
        {
            GamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            AudioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer),
                $"Attempted to pass an empty {typeof(AudioPlayer)}!");

            MasterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException(nameof(masterCameraHolder),
                $"Attempted to pass an empty {typeof(MasterCameraHolder)}!");

            MasterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException(nameof(masterCameraShaker),
                $"Attempted to pass an empty {typeof(MasterCameraShaker)}!");
        }

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
                GamePauser.Paused == true) return;

            _behaviour(Body, ref _data);
        }

        #region interfaces

        public async UniTask WaitForEscapeAsync(Func<bool> condition, CancellationToken token)
        {
            while (MasterCameraHolder.InsideViewport(transform.position) == false)
            {
                if (token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            while (MasterCameraHolder.InsideViewport(transform.position) == true)
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
            AudioPlayer.PlayOnceAsync(_config.CollisionAudio.Random, e.HitPosition, null, default, true).Forget();

            if (_config.CameraShakeOnCollisionEnabled == true)
                MasterCameraShaker.ShakeOnCollisionAsync().Forget();
        }

        #endregion
    }
}