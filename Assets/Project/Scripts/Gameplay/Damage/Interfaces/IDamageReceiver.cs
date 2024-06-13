using System;

namespace SpaceAce.Gameplay.Damage
{
    public interface IDamageReceiver
    {
        event EventHandler<DamageReceivedEventArgs> DamageReceived;
    }
}