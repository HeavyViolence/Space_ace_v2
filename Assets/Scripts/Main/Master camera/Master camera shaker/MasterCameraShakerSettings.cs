using Newtonsoft.Json;

using System;

using UnityEngine;

namespace SpaceAce.Main
{
    [Serializable]
    public sealed class MasterCameraShakerSettings
    {
        public static MasterCameraShakerSettings Default => new(ShakeSettings.Default,
                                                                ShakeSettings.Default,
                                                                ShakeSettings.Default,
                                                                ShakeSettings.Default);

        [SerializeField]
        private ShakeSettings _onShotFired;

        [SerializeField]
        private ShakeSettings _onDefeat;

        [SerializeField]
        private ShakeSettings _onCollision;

        [SerializeField]
        private ShakeSettings _onHit;

        [JsonIgnore]
        public ShakeSettings OnShotFired => _onShotFired;

        [JsonIgnore]
        public ShakeSettings OnDefeat => _onDefeat;

        [JsonIgnore]
        public ShakeSettings OnCollision => _onCollision;

        [JsonIgnore]
        public ShakeSettings OnHit => _onHit;

        public MasterCameraShakerSettings(ShakeSettings onShotFired,
                                          ShakeSettings onDefeat,
                                          ShakeSettings onCollision,
                                          ShakeSettings onHit)
        {
            _onShotFired = onShotFired ?? throw new ArgumentNullException(nameof(onShotFired),
                $"Attempted to pass an empty {typeof(ShakeSettings)}!");

            _onDefeat = onDefeat ?? throw new ArgumentNullException(nameof(onDefeat),
                $"Attempted to pass an empty {typeof(ShakeSettings)}!");

            _onCollision = onCollision ?? throw new ArgumentNullException(nameof(onCollision),
                $"Attempted to pass an empty {typeof(ShakeSettings)}!");

            _onHit = onHit ?? throw new ArgumentNullException(nameof(onHit),
                $"Attempted to pass an empty {typeof(ShakeSettings)}!");
        }
    }
}