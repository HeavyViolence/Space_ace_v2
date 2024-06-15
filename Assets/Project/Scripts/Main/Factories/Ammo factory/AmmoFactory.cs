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
                AmmoType.Cooling => new CoolingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<CoolingAmmoSetConfig>()),
                AmmoType.Explosive => new ExplosiveAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<ExplosiveAmmoSetConfig>()),
                AmmoType.Devastating => new DevastatingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<DevastatingAmmoSetConfig>()),
                AmmoType.Catalytic => new CatalyticAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<CatalyticAmmoSetConfig>()),
                AmmoType.Stabilizing => new StabilizingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<StabilizingAmmoSetConfig>()),
                AmmoType.Homing => new HomingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<HomingAmmoSetConfig>()),
                AmmoType.Piercing => new PiercingAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<PiercingAmmoSetConfig>()),
                AmmoType.Entangled => new EntangledAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<EntangledAmmoSetConfig>()),
                AmmoType.EMP => new EMPAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<EMPAmmoSetConfig>()),
                AmmoType.Nanite => new NaniteAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<NaniteAmmoSetConfig>()),
                AmmoType.Missiles => new MissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<MissileSetConfig>()),
                AmmoType.ExplosiveMissiles => new ExplosiveMissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<ExplosiveMissileSetConfig>()),
                AmmoType.NaniteMissiles => new NaniteMissileSet(_ammoServices, size, quality, _config.GetAmmoConfig<NaniteMissileSetConfig>()),
                AmmoType.Scatter => new ScatterAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<ScatterAmmoSetConfig>()),
                AmmoType.Antimatter => new AntimatterAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<AntimatterAmmoSetConfig>()),
                AmmoType.Stasis => new StasisAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<StasisAmmoSetConfig>()),
                AmmoType.Fusion => new FusionAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<FusionAmmoSetConfig>()),
                _ => new RegularAmmoSet(_ammoServices, size, quality, _config.GetAmmoConfig<RegularAmmoSetConfig>()),
            };
        }

        public AmmoSet Create(AmmoSetSavableState state)
        {
            if (state is null) throw new ArgumentNullException();

            if (state is RegularAmmoSetSavableState regularAmmoState)
                return new RegularAmmoSet(_ammoServices, _config.GetAmmoConfig<RegularAmmoSetConfig>(), regularAmmoState);

            if (state is StrangeAmmoSetSavableState strangeAmmoState)
                return new StrangeAmmoSet(_ammoServices, _config.GetAmmoConfig<StrangeAmmoSetConfig>(), strangeAmmoState);

            if (state is ClusterAmmoSetSavableState clusterAmmoState)
                return new ClusterAmmoSet(_ammoServices, _config.GetAmmoConfig<ClusterAmmoSetConfig>(), clusterAmmoState);

            if (state is CriticalAmmoSetSavableState criticalAmmoState)
                return new CriticalAmmoSet(_ammoServices, _config.GetAmmoConfig<CriticalAmmoSetConfig>(), criticalAmmoState);

            if (state is CoolingAmmoSetSavableState coolingAmmoState)
                return new CoolingAmmoSet(_ammoServices, _config.GetAmmoConfig<CoolingAmmoSetConfig>(), coolingAmmoState);

            if (state is ExplosiveAmmoSetSavableState explosiveAmmoState)
                return new ExplosiveAmmoSet(_ammoServices, _config.GetAmmoConfig<ExplosiveAmmoSetConfig>(), explosiveAmmoState);

            if (state is DevastatingAmmoSetSavableState devastatingAmmoState)
                return new DevastatingAmmoSet(_ammoServices, _config.GetAmmoConfig<DevastatingAmmoSetConfig>(), devastatingAmmoState);

            if (state is CatalyticAmmoSetSavableState catalyticAmmoState)
                return new CatalyticAmmoSet(_ammoServices, _config.GetAmmoConfig<CatalyticAmmoSetConfig>(), catalyticAmmoState);

            if (state is StabilizingAmmoSetSavableState stabilizingAmmoState)
                return new StabilizingAmmoSet(_ammoServices, _config.GetAmmoConfig<StabilizingAmmoSetConfig>(), stabilizingAmmoState);

            if (state is HomingAmmoSetSavableState homingAmmoState)
                return new HomingAmmoSet(_ammoServices, _config.GetAmmoConfig<HomingAmmoSetConfig>(), homingAmmoState);

            if (state is PiercingAmmoSetSavableState piercingAmmoState)
                return new PiercingAmmoSet(_ammoServices, _config.GetAmmoConfig<PiercingAmmoSetConfig>(), piercingAmmoState);

            if (state is EntangledAmmoSetSavableState entangledAmmoState)
                return new EntangledAmmoSet(_ammoServices, _config.GetAmmoConfig<EntangledAmmoSetConfig>(), entangledAmmoState);

            if (state is EMPAmmoSetSavableState empAmmoState)
                return new EMPAmmoSet(_ammoServices, _config.GetAmmoConfig<EMPAmmoSetConfig>(), empAmmoState);

            if (state is NaniteAmmoSetSavableState naniteAmmoState)
                return new NaniteAmmoSet(_ammoServices, _config.GetAmmoConfig<NaniteAmmoSetConfig>(), naniteAmmoState);

            if (state is MissileSetSavableState missileState)
                return new MissileSet(_ammoServices, _config.GetAmmoConfig<MissileSetConfig>(), missileState);

            if (state is ExplosiveMissileSetSavableState explosiveMissileState)
                return new ExplosiveMissileSet(_ammoServices, _config.GetAmmoConfig<ExplosiveMissileSetConfig>(), explosiveMissileState);

            if (state is NaniteMissileSetSavableState naniteMissilesState)
                return new NaniteMissileSet(_ammoServices, _config.GetAmmoConfig<NaniteMissileSetConfig>(), naniteMissilesState);

            if (state is ScatterAmmoSetSavableState scatterAmmoState)
                return new ScatterAmmoSet(_ammoServices, _config.GetAmmoConfig<ScatterAmmoSetConfig>(), scatterAmmoState);

            if (state is AntimatterAmmoSetSavableState antimatterAmmoState)
                return new AntimatterAmmoSet(_ammoServices, _config.GetAmmoConfig<AntimatterAmmoSetConfig>(), antimatterAmmoState);

            if (state is StasisAmmoSetSavableState stasisAmmoState)
                return new StasisAmmoSet(_ammoServices, _config.GetAmmoConfig<StasisAmmoSetConfig>(), stasisAmmoState);

            if (state is FusionAmmoSetSavableState fusionAmmoState)
                return new FusionAmmoSet(_ammoServices, _config.GetAmmoConfig<FusionAmmoSetConfig>(), fusionAmmoState);

            throw new Exception($"{nameof(AmmoFactoryConfig)} doesn't contain configs for all ammo types!");
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