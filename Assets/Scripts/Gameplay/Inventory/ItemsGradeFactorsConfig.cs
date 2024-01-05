using UnityEngine;

namespace SpaceAce.Gameplay.Inventories
{
    [CreateAssetMenu(fileName = "Items grade factors config",
                     menuName = "Space ace/Configs/Inventory/Items grade factors config")]
    public sealed class ItemsGradeFactorsConfig : ScriptableObject
    {
        public const float MinPriceFactorPerGrade = 1f;
        public const float MaxPriceFactorPerGrade = 2f;

        public const float MinPropertyFactorPerGrade = 1f;
        public const float MaxPropertyFactorPerGrade = 2f;

        [SerializeField, Range(MinPriceFactorPerGrade, MaxPriceFactorPerGrade)]
        private float _priceFactorPerGrade = MinPriceFactorPerGrade;

        public float PriceFactorPerGrade => _priceFactorPerGrade;

        public float GetPriceFactor(int grade) => Mathf.Pow(PriceFactorPerGrade, grade);

        [SerializeField, Range(MinPropertyFactorPerGrade, MaxPropertyFactorPerGrade)]
        private float _propertyFactorPerGrade = MinPropertyFactorPerGrade;

        public float PropertyFactorPerGrade => _propertyFactorPerGrade;

        public float GetPropertyFactor(int grade) => Mathf.Pow(PropertyFactorPerGrade, grade);
    }
}