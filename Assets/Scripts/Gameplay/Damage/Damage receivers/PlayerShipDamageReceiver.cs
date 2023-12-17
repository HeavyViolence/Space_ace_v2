using UnityEngine;

namespace SpaceAce.Gameplay.Damage
{
    [RequireComponent(typeof(PlayerShipDurability))]
    [RequireComponent(typeof(PlayerShipArmor))]
    public sealed class PlayerShipDamageReceiver : DamageReceiver
    {

    }
}