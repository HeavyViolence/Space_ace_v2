using SpaceAce.Gameplay.Inventories;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Main.Factories
{
    public sealed class AmmoFactory
    {
        private readonly AmmoConfigs _ammoConfigs;
        private readonly AmmoServices _ammoServices;

        public AmmoFactory(ProjectileFactory projectileFactory,
                           ProjectileHitEffectFactory projectileHitEffectFactory,
                           GameStateLoader gameStateLoader,
                           Localizer localizer,
                           MasterCameraHolder masterCameraHolder,
                           MasterCameraShaker masterCameraShaker,
                           AudioPlayer audioPlayer,
                           GamePauser gamePauser,
                           AmmoConfigs ammoConfigs)
        {
            _ammoServices = new(gameStateLoader,
                                localizer,
                                masterCameraHolder,
                                masterCameraShaker,
                                audioPlayer,
                                projectileFactory,
                                projectileHitEffectFactory,
                                gamePauser);

            if (ammoConfigs == null) throw new ArgumentNullException();
            _ammoConfigs = ammoConfigs;
        }

        public AmmoStack Create(AmmoFactoryRequest request) =>
            Create(request.Type, request.Size, request.Quality, request.Amount);

        public AmmoStack Create(AmmoType type, ItemSize size, ItemQuality quality, int amount)
        {
            Ammo ammo;

            switch (type)
            {
                case AmmoType.Regular:
                    {
                        var config = _ammoConfigs.RegularAmmoConfig;
                        ammo = new RegularAmmo(_ammoServices, size, quality, config);

                        break;
                    }
                case AmmoType.Strange:
                    {
                        var config = _ammoConfigs.StrangeAmmoConfig;
                        ammo = new StrangeAmmo(_ammoServices, size, quality, config);

                        break;
                    }
                default:
                    {
                        var config = _ammoConfigs.RegularAmmoConfig;
                        ammo = new RegularAmmo(_ammoServices, size, quality, config);

                        break;
                    }
            }

            AmmoStack stack = new(ammo, amount);
            return stack;
        }
    }
}