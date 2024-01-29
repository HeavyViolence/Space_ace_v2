using UnityEngine;

namespace SpaceAce.Main
{
    [CreateAssetMenu(fileName = "Master camera shaker config",
                     menuName = "Space ace/Configs/Main/Master camera shaker config")]
    public sealed class MasterCameraShakerConfig : ScriptableObject
    {
        [SerializeField]
        private ShakeSettings _onShotFired = ShakeSettings.Default;

        [SerializeField, Space]
        private ShakeSettings _onDefeat = ShakeSettings.Default;

        [SerializeField, Space]
        private ShakeSettings _onCollision = ShakeSettings.Default;

        [SerializeField, Space]
        private ShakeSettings _onHit = ShakeSettings.Default;

        public MasterCameraShakerSettings Settings => new(_onShotFired, _onDefeat, _onCollision, _onHit);
    }
}