using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main
{
    public sealed class AnimatorPauser : MonoBehaviour, IPausable
    {
        private GamePauser _gamePauser;
        private Animator _animator;

        [Inject]
        private void Construct(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");
        }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            _animator.speed = 1f;
            _gamePauser.Register(this);
        }

        private void OnDisable()
        {
            _gamePauser.Deregister(this);
        }

        public void Pause()
        {
            if (_animator != null) _animator.speed = 0f;
        }

        public void Resume()
        {
            if (_animator != null) _animator.speed = 1f;
        }
    }
}