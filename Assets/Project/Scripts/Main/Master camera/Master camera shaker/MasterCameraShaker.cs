using Newtonsoft.Json;

using SpaceAce.Auxiliary;
using SpaceAce.Main.Saving;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class MasterCameraShaker : IInitializable, IDisposable, ISavable, IFixedTickable
    {
        public event EventHandler SavingRequested;

        private readonly HashSet<ShakeRequest> _shakeRequests = new();
        private readonly HashSet<ShakeRequest> _shakeRequestsToBeDeleted = new();

        private readonly Rigidbody2D _masterCameraBody;

        private readonly GamePauser _gamePauser;
        private readonly ISavingSystem _savingSystem;

        private readonly AnimationCurve _shakeCurve;

        private MasterCameraShakerSettings _settings = MasterCameraShakerSettings.Default;

        public MasterCameraShakerSettings Settings
        {
            get => _settings;

            set
            {
                if (value is null) throw new ArgumentNullException();

                _settings = value;
                SavingRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public string SavedDataName => "Camera shaking";

        public MasterCameraShaker(MasterCameraShakerSettings settings,
                                  AnimationCurve shakeCurve,
                                  MasterCameraHolder masterCameraHolder,
                                  GamePauser gamePauser,
                                  ISavingSystem savingSystem)
        {
            _settings = settings ?? throw new ArgumentNullException();

            if (shakeCurve == null) throw new ArgumentNullException();

            _shakeCurve = shakeCurve;

            if (masterCameraHolder is null) throw new ArgumentNullException();

            Rigidbody2D body = masterCameraHolder.MasterCameraAnchor.gameObject.GetComponentInChildren<Rigidbody2D>();

            if (body == null) throw new MissingComponentException(nameof(Rigidbody2D));

            _masterCameraBody = body;

            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
        }

        public void ShakeOnShotFired() => _shakeRequests.Add(_settings.OnShotFired.NewShakeRequest);
        public void ShakeOnDefeat() => _shakeRequests.Add(_settings.OnDefeat.NewShakeRequest);
        public void ShakeOnCollision() => _shakeRequests.Add(_settings.OnCollision.NewShakeRequest);
        public void ShakeOnHit() => _shakeRequests.Add(_settings.OnHit.NewShakeRequest);

        private void ShakeMasterCamera()
        {
            if (_shakeRequests.Count == 0 || _gamePauser.Paused == true) return;

            float totalAmplitude = 0f;
            float averageAmplitude;

            float totalFrequency = 0f;
            float averageFrequency;

            int counter = 0;

            foreach (ShakeRequest request in _shakeRequests)
            {
                totalAmplitude += request.CurrentAmplitude;
                totalFrequency += request.CurrentFrequency;
                counter++;
            }

            averageAmplitude = totalAmplitude / counter;
            averageFrequency = totalFrequency / counter;

            float delta = averageAmplitude * Mathf.Sin(2f * Mathf.PI * averageFrequency);
            Vector2 position = new(AuxMath.RandomUnit * delta, AuxMath.RandomUnit * delta);

            _masterCameraBody.MovePosition(position);

            foreach (ShakeRequest request in _shakeRequests)
            {
                request.FixedUpdate(_shakeCurve);
                if (request.NormalizedDuration > 1f) _shakeRequestsToBeDeleted.Add(request);
            }

            if (_shakeRequestsToBeDeleted.Count > 0)
            {
                foreach (ShakeRequest request in _shakeRequestsToBeDeleted) _shakeRequests.Remove(request);
                _shakeRequestsToBeDeleted.Clear();
            }

            if (_shakeRequests.Count == 0) _masterCameraBody.position = Vector2.zero;
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
            catch (Exception) { }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && SavedDataName == other.SavedDataName;

        public override int GetHashCode() => SavedDataName.GetHashCode();

        public void FixedTick()
        {
            ShakeMasterCamera();
        }

        #endregion
    }
}