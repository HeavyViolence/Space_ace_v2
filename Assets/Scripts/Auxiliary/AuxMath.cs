using System.Collections.Generic;
using System.Linq;
using System;

using SpaceAce.Gameplay.Inventories;

using UnityEngine;

namespace SpaceAce.Auxiliary
{
    public static class AuxMath
    {
        public const float Phi = 1.618_033f;

        public static float RandomNormal => UnityEngine.Random.Range(0f, 1f);
        public static float RandomUnit => UnityEngine.Random.Range(-1f, 1f);
        public static float RandomSign => RandomUnit > 0f ? 1f : -1f;
        public static bool RandomBoolean => RandomSign > 0f;

        public static IEnumerable<int> GetRandomNumbersWithoutRepetition(int min, int max, int amount)
        {
            if (min >= max) throw new ArgumentOutOfRangeException();
            if (amount > max - min) throw new ArgumentOutOfRangeException();

            System.Random random = new();
            List<int> availableNumbers = Enumerable.Range(min, max - min).ToList();
            List<int> generatedNumbers = new(amount);

            for (int i = 0; i < amount; i++)
            {
                int index = random.Next(0, availableNumbers.Count);

                generatedNumbers.Add(availableNumbers[index]);
                availableNumbers.RemoveAt(index);
            }

            return generatedNumbers;
        }

        public static IEnumerable<int> GetRandomNumbersWithoutRepetition(IEnumerable<int> availableNumbers, int amount)
        {
            if (availableNumbers.Count() < amount) throw new ArgumentOutOfRangeException();

            System.Random random = new();
            HashSet<int> numbersToUse = new(availableNumbers);
            List<int> generatedNumbers = new(amount);

            for (int i = 0; i < amount; i++)
            {
                int index = random.Next(0, numbersToUse.Count);
                int randomNumberFromRange = numbersToUse.ElementAt(index);

                generatedNumbers.Add(randomNumberFromRange);
                numbersToUse.Remove(index);
            }

            return generatedNumbers;
        }

        public static float ModifyItemPropertyBySize(float value, float factorPerSize, ItemSize size)
        {
            return size switch
            {
                ItemSize.Small => value / factorPerSize,
                ItemSize.Medium => value,
                ItemSize.Large => value * factorPerSize,
                _ => value
            };
        }

        public static float ModifyItemPropertyByQuality(float value, float factorPerQuality, ItemQuality quality)
        {
            if (factorPerQuality >= 1f)
            {
                return quality switch
                {
                    ItemQuality.Poor => value / factorPerQuality,
                    ItemQuality.Common => value,
                    ItemQuality.Uncommon => value * factorPerQuality,
                    ItemQuality.Rare => value * Mathf.Pow(factorPerQuality, 2f),
                    ItemQuality.Exotic => value * Mathf.Pow(factorPerQuality, 3f),
                    ItemQuality.Epic => value * Mathf.Pow(factorPerQuality, 5f),
                    ItemQuality.Legendary => value * Mathf.Pow(factorPerQuality, 7f),
                    _ => value
                };
            }

            return quality switch
            {
                ItemQuality.Poor => value * factorPerQuality,
                ItemQuality.Common => value,
                ItemQuality.Uncommon => value / factorPerQuality,
                ItemQuality.Rare => value / Mathf.Pow(factorPerQuality, 2f),
                ItemQuality.Exotic => value / Mathf.Pow(factorPerQuality, 3f),
                ItemQuality.Epic => value / Mathf.Pow(factorPerQuality, 5f),
                ItemQuality.Legendary => value / Mathf.Pow(factorPerQuality, 7f),
                _ => value
            };
        }
    }
}