using SpaceAce.Gameplay.Damage;
using SpaceAce.Gameplay.Shooting;

namespace SpaceAce.UI
{
    public interface IEntityView
    {
        IDurabilityView DurabilityView { get; }
        IArmorView ArmorView { get; }
        IShooterView ShooterView { get; }
        IDestroyable Destroyable { get; }
    }
}