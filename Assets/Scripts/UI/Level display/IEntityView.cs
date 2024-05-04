using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Movement;
using SpaceAce.Gameplay.Shooting;

namespace SpaceAce.UI
{
    public interface IEntityView
    {
        IDurabilityView DurabilityView { get; }
        IArmorView ArmorView { get; }
        IShooterView ShooterView { get; }
        IEscapable Escapable { get; }
        IDestroyable Destroyable { get; }
    }
}