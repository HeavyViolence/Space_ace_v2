using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Saving;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShaker : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Rigidbody2D _masterCameraBody;

        private readonly GamePauser _gamePauser;
        private readonly ISavingSystem _savingSystem;

        public const float MinAmplitude = 0.1f;
        public const float MaxAmplitude = 1f;

        public const float MinAttenuation = 0.1f;
        public const float MaxAttenuation = 2f;

        public const float MinFrequency = 0.1f;
        public const float MaxFrequency = 10f;

        public const float AmplitudeCutoff = 0.01f;

        private MasterCameraShakerSettings _settings = MasterCameraShakerSettings.Default;

        public MasterCameraShakerSettings Settings
        {
            get => _settings;

            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value),
                        $"Attempted to pass an empty {typeof(MasterCameraShakerSettings)}!");

                _settings = value;
                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ID => "Camera shaking";

        public MasterCameraShaker(MasterCameraShakerSettings settings,
                                  MasterCameraHolder masterCameraHolder,
                                  GamePauser gamePauser,
                                  ISavingSystem savingSystem)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings),
                $"Attempted to pass an empty {typeof(MasterCameraShakerSettings)}!");

            if (masterCameraHolder is null)
                throw new ArgumentNullException(nameof(masterCameraHolder),
                    $"Attempted to pass an empty {typeof(MasterCameraHolder)}!");

            Rigidbody2D body = masterCameraHolder.MasterCameraObject.GetComponentInChildren<Rigidbody2D>();

            if (body == null)
                throw new MissingComponentException($"Passed master camera object is missing {typeof(Rigidbody2D)}!");

            _masterCameraBody = body;

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");
        }

        public async UniTask ShakeOnShotFiredAsync(CancellationToken token = default)
        {
            await ShakeAsync(Settings.OnShotFired, token);
        }

        public async UniTask ShakeOnDefeatAsync(CancellationToken token = default)
        {
            await ShakeAsync(Settings.OnDefeat, token);
        }

        public async UniTask ShakeOnCollisionAsync(CancellationToken token = default)
        {
            await ShakeAsync(Settings.OnCollision, token);
        }

        public async UniTask ShakeOnHitAsync(CancellationToken token = default)
        {
            await ShakeAsync(Settings.OnHit, token);
        }

        private async UniTask ShakeAsync(ShakeSettings settings, CancellationToken token)
        {
            if (settings.Enabled == false) return;

            float amplitude = settings.Amplitude;
            float attenuation = settings.Attenuation;
            float frequency = settings.Frequency;

            float timer = 0f;
            float duration = -1f * Mathf.Log(AmplitudeCutoff) / attenuation;

            while (timer < duration)
            {
                if (token != default && token.IsCancellationRequested == true) break;

                timer += Time.fixedDeltaTime;

                float delta = amplitude * Mathf.Exp(-1f * attenuation * timer) * Mathf.Sin(2f * Mathf.PI * frequency * timer);
                float deltaX = delta * AuxMath.RandomSign;
                float deltaY = delta * AuxMath.RandomSign;
                var deltaPos = new Vector2(deltaX, deltaY);

                _masterCameraBody.MovePosition(deltaPos);

                while (_gamePauser.Paused == true) await UniTask.NextFrame();

                await UniTask.WaitForFixedUpdate();
            }

            _masterCameraBody.MovePosition(Vector2.zero);
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
        }

        public string GetState() => JsonConvert.SerializeObject(Settings);

        public void SetState(string state)
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<MasterCameraShakerSettings>(state);
            }
            catch (Exception)
            {
                _settings = MasterCameraShakerSettings.Default;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}