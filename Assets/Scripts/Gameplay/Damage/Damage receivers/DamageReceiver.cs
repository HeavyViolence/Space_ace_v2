using Cysharp.Threading.Tasks;

using SpaceAce.Main;
using SpaceAce.Main.Factories;

using System;

using UnityEngine;

using Zenject;

namespace SpaceAce.Gameplay.Damage
{
    public abstract class DamageReceiver : MonoBehaviour, IDamageable, IDestroyable
    {
        public event EventHandler<DamageReceivedEventArgs> DamageReceived;
        public event EventHandler<DestroyedEventArgs> Destroyed;

        [SerializeField]
        private DamageReceiverConfig _config;

        private ExplosionFactory _explosionFactory;
        private GamePauser _gamePauser;
        private float _lifeTimer;

        protected virtual ExplosionSize ExplosionSize => _config.ExplosionSize;
        protected virtual bool ShakeOnDefeat => _config.ShakeOnDefeat;
        protected virtual bool Indestructible => _config.Indestructible;

        protected Durability Durability { get; private set; }
        protected Armor Armor { get; private set; }

        public Guid ID { get; private set; }

        [Inject]
        private void Construct(ExplosionFactory factory, GamePauser gamePauser)
        {
            _explosionFactory = factory ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();
        }

        protected virtual void Awake()
        {
            Durability = FindDurabilityComponent();
            Armor = FindArmorComponent();
            ID = Guid.NewGuid();
        }

        protected virtual void OnEnable()
        {
            _lifeTimer = 0f;
        }

        protected virtual void Update()
        {
            if (_gamePauser.Paused == true) return;

            _lifeTimer += Time.deltaTime;
        }

        private Durability FindDurabilityComponent()
        {
            if (gameObject.TryGetComponent(out Durability durability) == true) return durability;
            else throw new MissingComponentException($"Game object is missing {typeof(Durability)} component!");
        }

        private Armor FindArmorComponent()
        {
            if (gameObject.TryGetComponent(out Armor armor) == true) return armor;
            else throw new MissingComponentException($"Game object is missing {typeof(Armor)} component!");
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f) throw new ArgumentOutOfRangeException();

            float damageToBeDealt = Armor.GetReducedDamage(damage);

            Durability.ApplyDamage(damageToBeDealt);
            DamageReceived?.Invoke(this, new(damage, damageToBeDealt, transform.position));

            if (Durability.Value == 0f)
            {
                _explosionFactory.CreateAsync(ExplosionSize, transform.position).Forget();
                Destroyed?.Invoke(this, new(transform.position, _lifeTimer, 0f, 0f));
            }
        }
    }
}