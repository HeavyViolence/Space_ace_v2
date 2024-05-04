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

        private float _escapeDelay;
        private CancellationTokenSource _escapeCancellation;

        protected Transform Transform { get; private set; }
        protected GamePauser GamePauser { get; private set; }
        protected AudioPlayer AudioPlayer { get; private set; }
        protected MasterCameraHolder MasterCameraHolder {  get; private set; }
        protected MasterCameraShaker MasterCameraShaker { get; private set; }
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
            Transform = transform;
            Body = GetComponent<Rigidbody2D>();
        }

        protected virtual void OnEnable()
        {
            _escapeDelay = 0f;
            _escapeCancellation = new();

            WaitForEscapeAsync(_escapeCancellation.Token).Forget();
        }

        protected virtual void OnDisable()
        {
            _escapeCancellation.Cancel();
            _escapeCancellation.Dispose();

            _escapeCancellation = null;
            Escaped = null;
        }

        private async UniTask WaitForEscapeAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(() => MasterCameraHolder.InsideViewport(Transform.position) == true, PlayerLoopTiming.FixedUpdate, token);
            await UniTask.WaitForFixedUpdate(token);
            await UniTask.WaitUntil(() => MasterCameraHolder.InsideViewport(Transform.position) == false, PlayerLoopTiming.FixedUpdate, token);

            if (_escapeDelay > 0f)
            {
                float timer = 0f;

                while (timer < _escapeDelay)
                {
                    if (token.IsCancellationRequested == true) return;

                    timer += Time.fixedDeltaTime;

                    await UniTask.WaitUntil(() => GamePauser.Paused == false, PlayerLoopTiming.FixedUpdate, token);
                    await UniTask.WaitForFixedUpdate(token);
                }
            }

            Escaped?.Invoke(this, EventArgs.Empty);
        }

        public void SetEscapeDelay(float delay)
        {
            if (delay < 0f) throw new ArgumentOutOfRangeException();
            _escapeDelay = delay;
        }
    }
}