using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;

namespace SpaceAce.Main.Factories
{
    public readonly struct AmmoFactoryRequest
    {
        public AmmoType Type { get; }
        public Size Size { get; }
        public Quality Quality { get; }

        public AmmoFactoryRequest(AmmoType type,
                                  Size size,
                                  Quality quality)
        {
            Type = type;
            Size = size;
            Quality = quality;
        }
    }
}