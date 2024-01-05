using SpaceAce.Gameplay.Inventories;

using System;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public sealed class AmmoStack : ItemStack, IEquatable<AmmoStack>
    {
        public Ammo Ammo => Item as Ammo;

        public AmmoStack(Ammo item, int amount) : base(item, amount) { }

        public override ItemStackSavableState GetSavableState() =>
            new AmmoStackSavableState(Item.Size, Item.Quality, Amount, Ammo.Type, Ammo.Skin);

        #region interfaces

        public override bool Equals(ItemStack other) => other is not null && Equals(other as AmmoStack);

        public bool Equals(AmmoStack stack) => stack is not null && Ammo.Equals(stack.Ammo);

        public override int GetHashCode() => Item.GetHashCode();

        #endregion
    }
}