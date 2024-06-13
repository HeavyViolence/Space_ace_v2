using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public sealed class EntityView : MonoBehaviour, IEntityView
    {
        [SerializeField]
        private EntityViewConfig _config;

        public Guid ID { get; private set; }
        public Sprite Icon => _config.Icon;

        public IDurabilityView DurabilityView { get; private set; }
        public IArmorView ArmorView { get; private set; }
        public IShooterView ShooterView { get; private set; }
        public IEscapable Escapable { get; private set; }
        public IDamageReceiver DamageReceiver { get; private set; }
        public IDestroyable Destroyable { get; private set; }

        private void Awake()
        {
            ID = Guid.NewGuid();

            if (gameObject.TryGetComponent(out IDurabilityView durability) == false) throw new MissingComponentException(nameof(IDurabilityView));
            DurabilityView = durability;

            if (gameObject.TryGetComponent(out IArmorView armor) == false) throw new MissingComponentException(nameof(IArmorView));
            ArmorView = armor;

            if (_config.ShootingViewEnabled == true)
            {
                if (gameObject.TryGetComponent(out IShooterView shooting) == false) throw new MissingComponentException(nameof(IShooterView));
                ShooterView = shooting;
            }
            else
            {
                ShooterView = null;
            }

            if (gameObject.TryGetComponent(out IEscapable scapable) == false) throw new MissingComponentException(nameof(IEscapable));
            Escapable = scapable;

            if (gameObject.TryGetComponent(out IDestroyable destroyable) == false) throw new MissingComponentException(nameof(IDestroyable));
            Destroyable = destroyable;

            if (gameObject.TryGetComponent(out IDamageReceiver damageReceiver) == false) throw new MissingComponentException(nameof(IDamageReceiver));
            DamageReceiver = damageReceiver;
        }

        public async UniTask<string> GetLocalizedNameAsync() => await _config.GetLocalizedNameAsync();
    }
}