using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;

using System;

namespace SpaceAce.Main.Factories
{
    public sealed class AmmoFactory
    {
        private readonly AmmoSetConfigs _ammoConfigs;
        private readonly AmmoServices _ammoServices;

        public AmmoFactory(ProjectileFactory projectileFactory,
                           ProjectileHitEffectFactory projectileHitEffectFactory,
                           GameStateLoader gameStateLoader,
                           Localizer localizer,
                           MasterCameraHolder masterCameraHolder,
                           MasterCameraShaker masterCameraShaker,
                           AudioPlayer audioPlayer,
                           GamePauser gamePauser,
                           ItemPropertyEvaluator itemPropertyEvaluator,
                           AmmoSetConfigs ammoConfigs)
        {
            _ammoServices = new(gameStateLoader,
                                localizer,
                                masterCameraHolder,
                                masterCameraShaker,
                                audioPlayer,
                                projectileFactory,
                                projectileHitEffectFactory,
                                gamePauser,
                                itemPropertyEvaluator);

            if (ammoConfigs == null) throw new ArgumentNullException();
            _ammoConfigs = ammoConfigs;
        }

        public AmmoSet Create(AmmoFactoryRequest request) =>
            Create(request.Type, request.Size, request.Quality);

        public AmmoSet Create(AmmoType type, Size size, Quality quality)
        {
            AmmoSet ammo;

            switch (type)
            {
                case AmmoType.Regular:
                    {
                        var config = _ammoConfigs.RegularAmmoConfig;
                        ammo = new RegularAmmoSet(_ammoServices, size, quality, config);

                        break;
                    }
                case AmmoType.Strange:
                    {
                        var config = _ammoConfigs.StrangeAmmoConfig;
                        ammo = new StrangeAmmoSet(_ammoServices, size, quality, config);

                        break;
                    }
                default:
                    {
                        var config = _ammoConfigs.RegularAmmoConfig;
                        ammo = new RegularAmmoSet(_ammoServices, size, quality, config);

                        break;
                    }
            }

            return ammo;
        }

        public AmmoSet Create(AmmoSetSavableState savedState)
        {
            if (savedState is null) throw new ArgumentNullException();

            if (savedState is RegularAmmoSetSavableState regularAmmoSetState)
            {
                RegularAmmoSetConfig config = _ammoConfigs.RegularAmmoConfig;
                return new RegularAmmoSet(_ammoServices, config, regularAmmoSetState);
            }

            if (savedState is StrangeAmmoSetSavableState strangeAmmoSetState)
            {
                StrangeAmmoSetConfig config = _ammoConfigs.StrangeAmmoConfig;
                return new StrangeAmmoSet(_ammoServices, config, strangeAmmoSetState);
            }

            throw new NotImplementedException();
        }
    }
}