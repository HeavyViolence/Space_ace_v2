using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Effects;
using SpaceAce.Gameplay.Experience;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ExplosionFactories;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    [RequireComponent(typeof(Durability))]
    [RequireComponent(typeof(Armor))]
    [RequireComponent(typeof(ExperienceCollector))]
    public sealed class DamageReceiver : MonoBehaviour, IDamageable, IDamageReceiver, IDestroyable, INaniteTarget
    {
        public event EventHandler<DamageReceivedEventArgs> DamageReceived;
        public event EventHandler<DestroyedEventArgs> Destroyed;

        [SerializeField]
        private DamageReceiverConfig _config;

        private ExplosionFactory _explosionFactory;
        private GamePauser _gamePauser;
        private AudioPlayer _audioPlayer;
        private MasterCameraShaker _masterCameraShaker;
        private MasterCameraHolder _masterCameraHolder;
        private Transform _transform;
        private Durability _durability;
        private Armor _armor;
        private ExperienceCollector _experienceCollector;

        private float _lifeTime;

        private bool _invulnerable = false;
        private CancellationTokenSource _invulnerabilityCancellation;

        public Guid ID { get; private set; }

        [Inject]
        private void Construct(ExplosionFactory factory,
                               GamePauser gamePauser,
                               AudioPlayer audioPlayer,
                               MasterCameraShaker masterCameraShaker,
                               MasterCameraHolder masterCameraHolder)
        {
            _explosionFactory = factory ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _masterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
            _masterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            ID = Guid.NewGuid();

            _transform = transform;
            _durability = FindDurabilityComponent();
            _armor = FindArmorComponent();
            _experienceCollector = FindExperienceCollector();
        }

        private void OnEnable()
        {
            _lifeTime = 0f;
            
            if (_config.Invulnerable == true)
            {
                _invulnerabilityCancellation = new();
                MakeInvulnerableForDurationAsync(_config.InvulnerabilityDuration, _invulnerabilityCancellation.Token).Forget();
            }
        }

        private void OnDisable()
        {
            _invulnerabilityCancellation?.Cancel();
            _invulnerabilityCancellation?.Dispose();
            _invulnerabilityCancellation = null;
        }

        private void Update()
        {
            if (_gamePauser.Paused == true) return;
            _lifeTime += Time.deltaTime;
        }

        private Durability FindDurabilityComponent()
        {
            if (gameObject.TryGetComponent(out Durability durability) == true) return durability;
            throw new MissingComponentException($"Game object is missing {typeof(Durability)} component!");
        }

        private Armor FindArmorComponent()
        {
            if (gameObject.TryGetComponent(out Armor armor) == true) return armor;
            throw new MissingComponentException($"Game object is missing {typeof(Armor)} component!");
        }

        private ExperienceCollector FindExperienceCollector()
        {
            if (gameObject.TryGetComponent(out ExperienceCollector collector) == true) return collector;
            throw new MissingComponentException($"Game object is missing {typeof(ExperienceCollector)} component!");
        }

        private async UniTask MakeInvulnerableForDurationAsync(float duration, CancellationToken token)
        {
            _invulnerable = true;

            await UniTask.WaitUntil(() => _masterCameraHolder.InsideViewport(_transform.position) == true, PlayerLoopTiming.FixedUpdate, token);

            float timer = 0f;

            while (timer < duration)
            {
                if (token.IsCancellationRequested == true) break;

                timer += Time.fixedDeltaTime;

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.WaitForFixedUpdate();
            }

            _invulnerable = false;
        }

        public void ApplyDamage(float damage, float armorIgnoring = 0f)
        {
            if (gameObject.activeInHierarchy == false ||
                _masterCameraHolder.InsideViewport(_transform.position) == false ||
                _invulnerable == true)
            {
                return;
            }

            if (damage <= 0f) throw new ArgumentOutOfRangeException();
            if (armorIgnoring < 0f || armorIgnoring > 1f) throw new ArgumentOutOfRangeException();

            float damageToBeDealt = _armor.GetReducedDamage(damage, armorIgnoring);

            _durability.ApplyDamage(damageToBeDealt);
            DamageReceived?.Invoke(this, new(damage, damageToBeDealt, _transform.position));

            if (_durability.Value == 0f)
            {
                _explosionFactory.CreateAsync(_config.ExplosionSize, _transform.position).Forget();
                _masterCameraShaker.ShakeOnDefeat();
                _audioPlayer.PlayOnceAsync(_config.ExplosionAudio.Random, _transform.position, null, true).Forget();
                Destroyed?.Invoke(this, new(_transform.position, _lifeTime, _experienceCollector.GetExperience(_lifeTime)));
            }
        }

        #region nanite target interface

        public bool NanitesActive { get; private set; } = false;

        public async UniTask<bool> TryApplyNanitesAsync(Nanites nanites, CancellationToken token = default)
        {
            if (NanitesActive == true) return false;

            NanitesActive = true;
            float timer = 0f;

            while (timer < nanites.Duration)
            {
                if (token.IsCancellationRequested == true || gameObject.activeInHierarchy == false) break;

                timer += Time.fixedDeltaTime;

                _durability.ApplyDamage(nanites.DamagePerSecond * Time.fixedDeltaTime);

                await UniTask.WaitUntil(() => _gamePauser.Paused == false);
                await UniTask.WaitForFixedUpdate();
            }

            NanitesActive = false;
            return true;
        }

        #endregion
    }
}