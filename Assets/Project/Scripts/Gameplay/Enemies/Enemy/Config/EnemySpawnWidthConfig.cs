using SpaceAce.Auxiliary;

using UnityEngine;

namespace SpaceAce.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "Enemy spawn width config",
                     menuName = "Space ace/Configs/Enemies/Enemy spawn width config")]
    public sealed class EnemySpawnWidthConfig : ScriptableObject
    {
        [SerializeField]
        private AnimationCurve _spawnWidthProbability;

        public float GetSpawnWidth(float min, float max)
        {
            float widthNormalized = _spawnWidthProbability.Evaluate(AuxMath.RandomNormal);
            float width = Mathf.Lerp(min, max, widthNormalized);

            return width;
        }
    }
}