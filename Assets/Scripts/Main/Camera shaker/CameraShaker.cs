using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SpaceAce.Auxiliary;
using System;
using UnityEngine;
using Zenject;

namespace SpaceAce.Main
{
    public sealed class CameraShaker : MonoBehaviour, ISavable
    {
        public event EventHandler SavingRequested;

        [SerializeField] private Rigidbody2D _cameraRigidbody;

        private GamePauser _gamePauser = null;
        private SavingSystem _savingSystem = null;

        private int _activeShakers = 0;

        public const float MinAmplitude = 0.1f;
        public const float MaxAmplitude = 1f;

        public const float MinAttenuation = 0.1f;
        public const float MaxAttenuation = 2f;

        public const float MinFrequency = 0.1f;
        public const float MaxFrequency = 10f;

        public const float AmplitudeCutoff = 0.01f;

        private readonly Rigidbody2D _body;

        private CameraShakerSettings _settings = CameraShakerSettings.Default;
        public CameraShakerSettings Settings
        {
            get => _settings;

            set
            {
                _settings = value;
                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ID => "Camera shaking";

        [Inject]
        private void Construct(GamePauser gamePauser, SavingSystem savingSystem)
        {
            _gamePauser = gamePauser;
            _savingSystem = savingSystem;
        }

        private void OnEnable()
        {
            _savingSystem.Register(this);
        }

        private void OnDisable()
        {
            _savingSystem.Deregister(this);
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

                _body.MovePosition(deltaPos);

                while (_gamePauser.Paused) await UniTask.NextFrame();

                await UniTask.WaitForFixedUpdate();
            }

            if (--_activeShakers == 0) _body.MovePosition(Vector2.zero);
        }

        public string GetState() => JsonConvert.SerializeObject(Settings);

        public void SetState(string state)
        {
            try
            {
                _settings = JsonConvert.DeserializeObject<CameraShakerSettings>(state);
            }
            catch (Exception)
            {
                _settings = CameraShakerSettings.Default;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();
    }
}