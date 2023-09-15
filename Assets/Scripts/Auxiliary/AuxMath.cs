using System.Collections.Generic;
using System.Linq;
using System;

namespace SpaceAce.Auxiliary
{
    public static class AuxMath
    {
        public static float RandomNormal => UnityEngine.Random.Range(0f, 1f);
        public static float RandomSign => RandomNormal > 0.5f ? 1f : -1f;
        public static bool RandomBoolean => RandomSign > 0f;

        public static float GetRandom(float min, float max)
        {
            if (min > max) throw new ArgumentOutOfRangeException($"{nameof(min)}, {nameof(max)}");

            return UnityEngine.Random.Range(min, max);
        }

        public static int GetRandom(int minInclusive, int maxExclusive)
        {
            if (minInclusive > maxExclusive) throw new ArgumentOutOfRangeException($"{nameof(minInclusive)}, {nameof(maxExclusive)}");

            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        public static IEnumerable<int> GetRandomNumbersWithoutRepetition(int min, int max, int amount)
        {
            if (min >= max) throw new ArgumentOutOfRangeException($"{nameof(min)}, {nameof(max)}");
            if (amount > max - min) throw new ArgumentOutOfRangeException(nameof(amount));

            Random random = new();
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
            if (availableNumbers.Count() < amount) throw new ArgumentOutOfRangeException(nameof(amount));

            Random random = new();
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
    }
}