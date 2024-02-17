using SpaceAce.Gameplay.Items;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
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
        public MasterCameraShaker MasterCameraShaker { get; }
        public AudioPlayer AudioPlayer { get; }
        public ProjectileFactory ProjectileFactory { get; }
        public ProjectileHitEffectFactory ProjectileHitEffectFactory { get; }
        public GamePauser GamePauser { get; }
        public ItemPropertyEvaluator ItemPropertyEvaluator { get; }

        public AmmoServices(GameStateLoader gameStateLoader,
                            Localizer localizer,
                            MasterCameraHolder masterCameraHolder,
                            MasterCameraShaker masterCameraShaker,
                            AudioPlayer audioPlayer,
                            ProjectileFactory projectileFactory,
                            ProjectileHitEffectFactory projectileHitEffectFactory,
                            GamePauser gamePauser,
                            ItemPropertyEvaluator itemPropertyEvaluator)
        {
            GameStateLoader = gameStateLoader ?? throw new ArgumentNullException();
            Localizer = localizer ?? throw new ArgumentNullException();
            MasterCameraHolder = masterCameraHolder ?? throw new ArgumentNullException();
            MasterCameraShaker = masterCameraShaker ?? throw new ArgumentNullException();
            AudioPlayer = audioPlayer ?? throw new ArgumentNullException();
            ProjectileFactory = projectileFactory ?? throw new ArgumentNullException();
            ProjectileHitEffectFactory = projectileHitEffectFactory ?? throw new ArgumentNullException();
            GamePauser = gamePauser ?? throw new ArgumentNullException();
            ItemPropertyEvaluator = itemPropertyEvaluator ?? throw new ArgumentNullException();
        }
    }
}