using SpaceAce.Gameplay.Items;
using SpaceAce.Gameplay.Shooting.Ammo;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Factories.ProjectileFactories;
using SpaceAce.Main.Factories.ProjectileHitEffectFactories;
using SpaceAce.Main.Localization;
using SpaceAce.UI;

using System;

namespace SpaceAce.Main.Factories.AmmoFactories
{
    public sealed class AmmoFactory
    {
        private readonly AmmoFactoryConfig _config;
        private readonly AmmoServices _ammoServices;
        private readonly ItemQualityToSpawnProbabilityConverter _itemQualityToSpawnProbabilityConverter;
        private readonly Random _random;

        public AmmoFactory(ProjectileFactory projectileFactory,
                           ProjectileHitEffectFactory projectileHitEffectFactory,
                           GameStateLoader gameStateLoader,
                           Localizer localizer,
                           MasterCameraHolder masterCameraHolder,
                           MasterCameraShaker masterCameraShaker,
                           AudioPlayer audioPlayer,
                           GamePauser gamePauser,
                           ItemPropertyEvaluator itemPropertyEvaluator,
                           ItemQualityToSpawnProbabilityConverter itemQualityToSpawnProbabilityConverter,
                           ItemIconProvider itemIconProvider,
                           AmmoFactoryConfig config)
        {
            _ammoServices = new(gameStateLoader,
                                localizer,
                                masterCameraHolder,
                                masterCameraShaker,
                                audioPlayer,
                                projectileFactory,
                                projectileHitEffectFactory,
                                gamePauser,
                                itemPropertyEvaluator,
                                itemIconProvider);

            if (config == null) throw new ArgumentNullException();
            _config = config;

            _itemQualityToSpawnProbabilityConverter = itemQualityToSpawnProbabilityConverter ?? throw new ArgumentNullException();
            _random = new();
        }

        public AmmoSet Create(AmmoFactoryRequest request) => Create(request.Type, request.Size, request.Quality);

        public AmmoSet Create(AmmoType type, Size size, Quality quality)
        {
            return type switch
            {
                AmmoType.Regular => new RegularAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<RegularAmmoSetConfig>()),
                AmmoType.Strange => new StrangeAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<StrangeAmmoSetConfig>()),
                AmmoType.Cluster => new ClusterAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<ClusterAmmoSetConfig>()),
                AmmoType.Critical => new CriticalAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<CriticalAmmoSetConfig>()),
                AmmoType.Explosive => new ExplosiveAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<ExplosiveAmmoSetConfig>()),
                AmmoType.Devastating => new DevastatingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<DevastatingAmmoSetConfig>()),
                AmmoType.Catalytic => new CatalyticAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<CatalyticAmmoSetConfig>()),
                AmmoType.Cooling => new CoolingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<CoolingAmmoSetConfig>()),
                AmmoType.Stabilizing => new StabilizingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<StabilizingAmmoSetConfig>()),
                AmmoType.Homing => new HomingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<HomingAmmoSetConfig>()),
                AmmoType.Piercing => new PiercingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<PiercingAmmoSetConfig>()),
                AmmoType.Entangled => new EntangledAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<EntangledAmmoSetConfig>()),
                AmmoType.EMP => new EMPAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<EMPAmmoSetConfig>()),
                AmmoType.Nanite => new NaniteAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<NaniteAmmoSetConfig>()),
                AmmoType.Missiles => new MissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<MissileSetConfig>()),
                AmmoType.ExplosiveMissiles => new ExplosiveMissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<ExplosiveMissileSetConfig>()),
                AmmoType.NaniteMissiles => new NaniteMissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<NaniteMissileSetConfig>()),
                _ => new RegularAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<RegularAmmoSetConfig>()),
            };
        }

        public AmmoSet Create(AmmoSetSavableState state)
        {
            if (state is null) throw new ArgumentNullException();

            if (state is RegularAmmoSetSavableState regularAmmoSetState)
                return new RegularAmmoSet(_ammoServices, _config.GetAmmoConfig<RegularAmmoSetConfig>(), regularAmmoSetState);

            if (state is StrangeAmmoSetSavableState strangeAmmoSetState)
                return new StrangeAmmoSet(_ammoServices, _config.GetAmmoConfig<StrangeAmmoSetConfig>(), strangeAmmoSetState);

            if (state is ClusterAmmoSetSavableState clusterAmmoSetState)
                return new ClusterAmmoSet(_ammoServices, _config.GetAmmoConfig<ClusterAmmoSetConfig>(), clusterAmmoSetState);

            if (state is CriticalAmmoSetSavableState criticalAmmoSetState)
                return new CriticalAmmoSet(_ammoServices, _config.GetAmmoConfig<CriticalAmmoSetConfig>(), criticalAmmoSetState);

            if (state is ExplosiveAmmoSetSavableState explosiveAmmoSetState)
                return new ExplosiveAmmoSet(_ammoServices, _config.GetAmmoConfig<ExplosiveAmmoSetConfig>(), explosiveAmmoSetState);

            if (state is DevastatingAmmoSetSavableState devastatingAmmoSetState)
                return new DevastatingAmmoSet(_ammoServices, _config.GetAmmoConfig<DevastatingAmmoSetConfig>(), devastatingAmmoSetState);

            if (state is CatalyticAmmoSetSavableState catalyticAmmoSetState)
                return new CatalyticAmmoSet(_ammoServices, _config.GetAmmoConfig<CatalyticAmmoSetConfig>(), catalyticAmmoSetState);

            if (state is CoolingAmmoSetSavableState cooolingAmmoSetState)
                return new CoolingAmmoSet(_ammoServices, _config.GetAmmoConfig<CoolingAmmoSetConfig>(), cooolingAmmoSetState);

            if (state is StabilizingAmmoSetSavableState stabilizingAmmoSetState)
                return new StabilizingAmmoSet(_ammoServices, _config.GetAmmoConfig<StabilizingAmmoSetConfig>(), stabilizingAmmoSetState);

            if (state is HomingAmmoSetSavableState homingAmmoSetState)
                return new HomingAmmoSet(_ammoServices, _config.GetAmmoConfig<HomingAmmoSetConfig>(), homingAmmoSetState);

            if (state is PiercingAmmoSetSavableState piercingAmmoSetState)
                return new PiercingAmmoSet(_ammoServices, _config.GetAmmoConfig<PiercingAmmoSetConfig>(), piercingAmmoSetState);

            if (state is EntangledAmmoSetSavableState entangledAmmoSetState)
                return new EntangledAmmoSet(_ammoServices, _config.GetAmmoConfig<EntangledAmmoSetConfig>(), entangledAmmoSetState);

            if (state is EMPAmmoSetSavableState empAmmoSetState)
                return new EMPAmmoSet(_ammoServices, _config.GetAmmoConfig<EMPAmmoSetConfig>(), empAmmoSetState);

            if (state is NaniteAmmoSetSavableState naniteAmmoSetState)
                return new NaniteAmmoSet(_ammoServices, _config.GetAmmoConfig<NaniteAmmoSetConfig>(), naniteAmmoSetState);

            if (state is MissileSetSavableState missileSetState)
                return new MissileSet(_ammoServices, _config.GetAmmoConfig<MissileSetConfig>(), missileSetState);

            if (state is ExplosiveMissileSetSavableState explosiveMissileSetState)
                return new ExplosiveMissileSet(_ammoServices, _config.GetAmmoConfig<ExplosiveMissileSetConfig>(), explosiveMissileSetState);

            if (state is NaniteMissileSetSavableState naniteMissileSetSavableState)
                return new NaniteMissileSet(_ammoServices, _config.GetAmmoConfig<NaniteMissileSetConfig>(), naniteMissileSetSavableState);

            throw new NotImplementedException();
        }

        public AmmoSet CreateProbable()
        {
            AmmoType probableType = _config.GetProbableAmmoType();

            Array availableSizes = Enum.GetValues(typeof(Size));
            int randomIndex = _random.Next(0, availableSizes.Length);
            Size randomSize = (Size)availableSizes.GetValue(randomIndex);

            Quality probableQuality = _itemQualityToSpawnProbabilityConverter.GetProbableQuality();

            return Create(probableType, randomSize, probableQuality);
        }
    }
}