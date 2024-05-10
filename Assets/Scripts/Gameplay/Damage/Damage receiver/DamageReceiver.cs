using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Experience;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ExplosionFactories;
using SpaceAce.UI;

using System;
using System.Threading;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    [RequireComponent(typeof(Durability))]
    [RequireComponent(typeof(Armor))]
    [RequireComponent(typeof(ExperienceCollector))]
    public sealed class DamageReceiver : MonoBehaviour, IDamageable,
                                                        IDestroyable,
                                                        IEntityView,
                                                        INaniteTarget
    {
        public event EventHandler<DamageReceivedEventArgs> DamageReceived;
        public event EventHandler<DestroyedEventArgs> Destroyed;

        [SerializeField]
        private DamageReceiverConfig _config;

        private ExplosionFactory _explosionFactory;
        private GamePauser _gamePauser;
        private AudioPlayer _audioPlayer;
        private MasterCameraShaker _masterCameraShaker;
        private float _lifeTime;

        private Transform _transform;
        private Durability _durability;
        private Armor _armor;
        private ExperienceCollector _experienceCollector;

        public Guid ID { get; private set; }

        public IDurabilityView DurabilityView => _durability;
        public IArmorView ArmorView => _armor;
        public IShooterView ShooterView { get; private set; }
        public IEscapable Escapable { get; private set; }
        public IDestroyable Destroyable => this;

        [Inject]
        private void Construct(ExplosionFactory factory,
                               GamePauser gamePauser,
                               AudioPlayer audioPlayer,
                               MasterCameraShaker masterCameraShaker)
        {
            _explosionFactory = factory ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException();
            _masterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
        }

        private void Awake()
        {
            ID = Guid.NewGuid();

            _transform = transform;
            _durability = FindDurabilityComponent();
            _armor = FindArmorComponent();
            _experienceCollector = FindExperienceCollector();
            ShooterView = FindShooterComponent();
            Escapable = FindEscapableComponent();
        }

        private void OnEnable()
        {
            _lifeTime = 0f;
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

        private Shooting.Shooting FindShooterComponent()
        {
            if (gameObject.TryGetComponent(out Shooting.Shooting shooting) == true) return shooting;
            return null;
        }

        private IEscapable FindEscapableComponent()
        {
            if (gameObject.TryGetComponent(out IEscapable escapable) == true) return escapable;
            throw new MissingComponentException($"Game object is missing {typeof(IEscapable)} component!");
        }

        private ExperienceCollector FindExperienceCollector()
        {
            if (gameObject.TryGetComponent(out ExperienceCollector collector) == true) return collector;
            throw new MissingComponentException($"Game object is missing {typeof(ExperienceCollector)} component!");
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f) throw new ArgumentOutOfRangeException();

            float damageToBeDealt = _armor.GetReducedDamage(damage);

            _durability.ApplyDamage(damageToBeDealt);
            DamageReceived?.Invoke(this, new(damage, damageToBeDealt, transform.position));

            if (_durability.Value == 0f)
            {
                _explosionFactory.CreateAsync(_config.ExplosionSize, transform.position).Forget();
                _masterCameraShaker.ShakeOnDefeat();
                _audioPlayer.PlayOnceAsync(_config.ExplosionAudio.Random, _transform.position, null, true).Forget();
                Destroyed?.Invoke(this, new(_transform.position, _lifeTime, _experienceCollector.GetExperience(_lifeTime)));
            }
        }

        #region nanite target interface

        private bool _nanitesActive = false;

        public async UniTask<bool> TryApplyNanitesAsync(Nanites nanites, CancellationToken token = default)
        {
            if (_nanitesActive == true) return false;

            _nanitesActive = true;
            float timer = 0f;

            while (timer < nanites.DamageDuration)
            {
                if (gameObject == null) return false;

                if (token.IsCancellationRequested == true ||
                    gameObject.activeInHierarchy == false)
                {
                    _nanitesActive = false;
                    return false;
                }

                if (_gamePauser.Paused == true) await UniTask.Yield();

                timer += Time.deltaTime;

                float damage = nanites.DamagePerSecond * Time.deltaTime;

                _durability.ApplyDamage(damage);
                DamageReceived?.Invoke(this, new(damage, damage, transform.position));

                if (_durability.Value == 0f)
                {
                    _nanitesActive = false;
                    _explosionFactory.CreateAsync(_config.ExplosionSize, transform.position).Forget();
                    Destroyed?.Invoke(this, new(_transform.position, _lifeTime, _experienceCollector.GetExperience(_lifeTime)));

                    return true;
                }

                await UniTask.Yield();
            }

            _nanitesActive = false;
            return true;
        }

        #endregion
    }
}