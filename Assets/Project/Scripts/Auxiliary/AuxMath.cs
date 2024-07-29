using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;

namespace SpaceAce.Auxiliary
{
    public static class AuxMath
    {
        public const float Phi = 1.618_033f;
        public const float E = 2.718_281f;
        public const float RPMtoDPS = 6f;
        public const float DegreesPerRotation = 360f;

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

        public static Dictionary<Vector2, T> InterpolateEnumByRange<T>(AnimationCurve interpolation, IEnumerable<T> customOrder = null) where T : Enum
        {
            if (interpolation == null) throw new ArgumentNullException();

            T[] members;

            if (customOrder is null) members = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            else members = customOrder.ToArray();

            Dictionary<Vector2, T> result = new(members.Length);

            for (int i = 0; i < members.Length; i++)
            {
                float rangeStart = interpolation.Evaluate((float)i / members.Length);
                float rangeEnd = interpolation.Evaluate((float)(i + 1) / members.Length);

                Vector2 range = new(rangeStart, rangeEnd);
                T member = members[i];

                result.Add(range, member);
            }

            return result;
        }

        public static Dictionary<Vector2, T> InterpolateValuesByRange<T>(AnimationCurve interpolation, IEnumerable<T> values)
        {
            if (interpolation == null) throw new ArgumentNullException();
            if (values is null) throw new ArgumentNullException();

            int valuesAmount = values.Count();
            int counter = 0;

            Dictionary<Vector2, T> result = new(valuesAmount);

            foreach (T value in values)
            {
                float rangeStart = interpolation.Evaluate((float)counter / valuesAmount);
                float rangeEnd = interpolation.Evaluate((float)(counter + 1) / valuesAmount);

                Vector2 range = new(rangeStart, rangeEnd);
                result.Add(range, value);

                counter++;
            }

            return result;
        }

        public static Dictionary<T, Vector2> InterpolateRangeByEnum<T>(AnimationCurve interpolation, IEnumerable<T> customOrder = null) where T : Enum
        {
            if (interpolation == null) throw new ArgumentNullException();

            T[] members;

            if (customOrder is null) members = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            else members = customOrder.ToArray();

            Dictionary<T, Vector2> result = new(members.Length);

            for (int i = 0; i < members.Length; i++)
            {
                float rangeStart = interpolation.Evaluate((float)i / members.Length);
                float rangeEnd = interpolation.Evaluate((float)(i + 1) / members.Length);

                Vector2 range = new(rangeStart, rangeEnd);
                T member = members[i];

                result.Add(member, range);
            }

            return result;
        }
    }
}