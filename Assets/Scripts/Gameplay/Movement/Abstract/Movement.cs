using Cysharp.Threading.Tasks;

using SpaceAce.Main;
using SpaceAce.Main.Audio;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Movement : MonoBehaviour, IEscapable
    {
        public event EventHandler Escaped;

        protected GamePauser GamePauser;
        protected AudioPlayer AudioPlayer;
        protected MasterCameraHolder MasterCameraHolder;
        protected MasterCameraShaker MasterCameraShaker;

        protected Rigidbody2D Body { get; private set; }

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
        }

        public async UniTask WaitForEscapeAsync(Func<bool> condition, float delay, CancellationToken token = default)
        {
            while (MasterCameraHolder.InsideViewport(transform.position) == false)
            {
                if (token != default && token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            while (MasterCameraHolder.InsideViewport(transform.position) == true)
            {
                if (token != default && token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            while (condition() == false)
            {
                if (token != default && token.IsCancellationRequested == true) return;
                await UniTask.Yield();
            }

            if (delay > 0f)
            {
                float escapeDelayTimer = 0f;

                while (escapeDelayTimer < delay)
                {
                    escapeDelayTimer += Time.deltaTime;

                    if (token != default && token.IsCancellationRequested == true) return;
                    while (GamePauser.Paused == true) await UniTask.Yield();

                    await UniTask.Yield();
                }
            }

            Escaped?.Invoke(this, EventArgs.Empty);
        }
    }
}