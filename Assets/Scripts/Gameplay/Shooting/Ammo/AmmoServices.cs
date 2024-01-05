using SpaceAce.Main;
using SpaceAce.Main.Factories;
using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public readonly struct AmmoServices
    {
        public GameStateLoader GameStateLoader { get; }
        public Localizer Localizer { get; }
        public MasterCameraHolder MasterCameraHolder { get; }
        public ProjectileFactory ProjectileFactory { get; }
        public GamePauser GamePauser { get; }

        public AmmoServices(GameStateLoader gameStateLoader,
                            Localizer localizer,
                            MasterCameraHolder masterCameraHolder,
                            ProjectileFactory projectileFactory,
                            GamePauser gamePauser)
        {
            GameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            Localizer = localizer ?? throw new ArgumentNullException();
            MasterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            ProjectileFactory = projectileFactory ?? throw new ArgumentNullException();
            GamePauser = gamePauser ?? throw new ArgumentNullException();
        }
    }
}