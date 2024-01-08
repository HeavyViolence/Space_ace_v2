using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class ParticleSystemPauser : MonoBehaviour, IPausable
    {
        public const float MinDuration = 0f;
        public const float MaxDuration = 10f;

        private GamePauser _gamePauser;
        private IEnumerable<ParticleSystem> _particleSystems;

        [SerializeField, Range(MinDuration, MaxDuration)]
        private float _effectDuration;

        public float EffectDuration => _effectDuration;

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        private void OnEnable()
        {
            _gamePauser.Register(this);
        }

        private void OnDisable()
        {
            _gamePauser.Deregister(this);
        }

        public void Pause()
        {
            if (_particleSystems is not null)
                foreach (var p in _particleSystems)
                    p.Pause();
        }

        public void Resume()
        {
            if (_particleSystems is not null)
                foreach (var p in _particleSystems)
                    p.Play();
        }
    }
}