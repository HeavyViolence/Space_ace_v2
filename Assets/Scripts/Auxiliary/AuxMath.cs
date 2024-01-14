using System.Collections.Generic;
using System.Linq;
using System;

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

        public static bool ValueInRange(Vector2 range, float value) => value >= range.x && value <= range.y;

        public static bool ValueInRange(float min, float max, float value) => value >= min && value <= max;
    }
}