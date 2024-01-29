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

        [SerializeField, JsonIgnore]
        private ShakeSettings _onShotFired;

        [SerializeField, JsonIgnore]
        private ShakeSettings _onDefeat;

        [SerializeField, JsonIgnore]
        private ShakeSettings _onCollision;

        [SerializeField, JsonIgnore]
        private ShakeSettings _onHit;

        public ShakeSettings OnShotFired => _onShotFired;
        public ShakeSettings OnDefeat => _onDefeat;
        public ShakeSettings OnCollision => _onCollision;
        public ShakeSettings OnHit => _onHit;

        public MasterCameraShakerSettings(ShakeSettings onShotFired,
                                          ShakeSettings onDefeat,
                                          ShakeSettings onCollision,
                                          ShakeSettings onHit)
        {
            _onShotFired = onShotFired ?? throw new ArgumentNullException();
            _onDefeat = onDefeat ?? throw new ArgumentNullException();
            _onCollision = onCollision ?? throw new ArgumentNullException();
            _onHit = onHit ?? throw new ArgumentNullException();
        }
    }
}