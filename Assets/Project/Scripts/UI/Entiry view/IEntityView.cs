using Cysharp.Threading.Tasks;

using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting;

using System;

using UnityEngine;

namespace SpaceAce.UI
{
    public interface IEntityView
    {
        Guid ID { get; }
        Sprite Icon { get; }

        IDurabilityView DurabilityView { get; }
        IArmorView ArmorView { get; }
        IShooterView ShooterView { get; }
        IEscapable Escapable { get; }
        IDamageReceiver DamageReceiver { get; }
        IDestroyable Destroyable { get; }

        UniTask<string> GetLocalizedNameAsync();
    }
}