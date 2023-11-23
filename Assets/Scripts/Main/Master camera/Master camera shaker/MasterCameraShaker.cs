using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Saving;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShaker : IMasterCameraShaker, IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly Rigidbody2D _masterCameraBody;

        private readonly IGamePauser _gamePauser;
        private readonly ISavingSystem _savingSystem;

        private int _activeShakers = 0;

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

        public MasterCameraShaker(IMasterCameraHolder masterCameraHolder,
                                  IGamePauser gamePauser,
                                  ISavingSystem savingSystem)
        {
            if (masterCameraHolder is null)
                throw new ArgumentNullException(nameof(masterCameraHolder),
                    $"Attempted to pass an empty {typeof(IMasterCameraHolder)}!");

            Rigidbody2D body = masterCameraHolder.MasterCameraObject.GetComponentInChildren<Rigidbody2D>();

            if (body == null)
                throw new MissingComponentException($"Passed master camera object is missing {typeof(Rigidbody2D)}!");

            _masterCameraBody = body;

            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(IGamePauser)}!");

            _savingSystem = savingSystem ?? throw new ArgumentNullException(nameof(savingSystem),
                $"Attempted to pass an empty {typeof(ISavingSystem)}!");
        }

        public void ShakeOnShotFired()
        {
            if (Settings.ShakingOnShotFiredEnabled == false) return;

            Shake(0.05f, 2f, 2f).Forget();
        }

        public void ShakeOnDefeat()
        {
            if (Settings.ShakingOnDefeatEnabled == false) return;

            Shake(0.5f, 2f, 1f).Forget();
        }

        public void ShakeOnCollision()
        {
            if (Settings.ShakingOnCollisionEnabled == false) return;

            Shake(0.2f, 2f, 2f).Forget();
        }

        public void ShakeOnHit()
        {
            if (Settings.ShakingOnHitEnabled == false) return;

            Shake(0.1f, 2f, 2f).Forget();
        }

        private async UniTaskVoid Shake(float amplitude, float attenuation, float frequency)
        {
            _activeShakers++;

            amplitude = Mathf.Clamp(amplitude, 0f, MaxAmplitude);
            attenuation = Mathf.Clamp(attenuation, 0f, MaxAttenuation);
            frequency = Mathf.Clamp(frequency, 0f, MaxFrequency);

            float timer = 0f;
            float duration = -1f * Mathf.Log(AmplitudeCutoff) / attenuation;

            while (timer < duration)
            {
                timer += Time.fixedDeltaTime;

                float delta = amplitude * Mathf.Exp(-1f * attenuation * timer) * Mathf.Sin(2f * Mathf.PI * frequency * timer);
                float deltaX = delta * AuxMath.RandomSign;
                float deltaY = delta * AuxMath.RandomSign;
                var deltaPos = new Vector2(deltaX, deltaY);

                _masterCameraBody.MovePosition(deltaPos);

                while (_gamePauser.Paused) await UniTask.NextFrame();

                await UniTask.WaitForFixedUpdate();
            }

            if (--_activeShakers == 0)
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