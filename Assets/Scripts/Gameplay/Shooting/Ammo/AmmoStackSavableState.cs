using SpaceAce.Gameplay.Inventories;
using SpaceAce.Main.Factories;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AmmoStackSavableState : ItemStackSavableState
    {
        public AmmoType Type { get; }
        public ProjectileSkin ProjectileSkin { get; }
        public ProjectileHitEffectSkin HitEffectSkin { get; }

        public AmmoStackSavableState(ItemSize size,
                                     ItemQuality quality,
                                     int amount,
                                     AmmoType type,
                                     ProjectileSkin projectileSkin,
                                     ProjectileHitEffectSkin hitEffectSkin) : base(size, quality, amount)
        {
            Type = type;
            ProjectileSkin = projectileSkin;
            HitEffectSkin = hitEffectSkin;
        }

        public override ItemStack Recreate(SavedItemsServices services) =>
            services.AmmoFactory.Create(Type, Size, Quality, ProjectileSkin, HitEffectSkin, Amount);
    }
}