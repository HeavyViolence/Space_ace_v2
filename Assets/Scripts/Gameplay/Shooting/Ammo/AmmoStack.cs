using SpaceAce.Gameplay.Inventories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AmmoStack : ItemStack
    {
        public AmmoStack(Ammo item, int amount) : base(item, amount) { }

        public override ItemStackSavableState GetSavableState()
        {
            var ammo = Item as Ammo;
            return new AmmoStackSavableState(ammo.Size, ammo.Quality, Amount, ammo.Type, ammo.ProjectileSkin, ammo.HitEffectSkin);
        }
    }
}