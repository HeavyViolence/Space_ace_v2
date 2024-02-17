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
            GamePauser = gamePauser ?? throw new ArgumentNullException();
            AudioPlayer = audioPlayer ?? throw new ArgumentNullException();
            MasterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            MasterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
        }

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
        }

        protected virtual void OnDisable()
        {
            Escaped = null;
        }

        public async UniTask WaitForEscapeAsync(float delay = 0f, CancellationToken token = default)
        {
            await UniTask.WaitUntil(() => MasterCameraHolder.InsideViewport(transform.position) == true, PlayerLoopTiming.FixedUpdate, token);
            await UniTask.WaitUntil(() => MasterCameraHolder.InsideViewport(transform.position) == false, PlayerLoopTiming.FixedUpdate, token);

            if (delay > 0f)
            {
                float timer = 0f;

                while (timer <= delay)
                {
                    timer += Time.fixedDeltaTime;

                    if (token.IsCancellationRequested == true) return;
                    while (GamePauser.Paused == true) await UniTask.WaitForFixedUpdate();

                    await UniTask.WaitForFixedUpdate();
                }
            }

            Escaped?.Invoke(this, EventArgs.Empty);
        }
    }
}