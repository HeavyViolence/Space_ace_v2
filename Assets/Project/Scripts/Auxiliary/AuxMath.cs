using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Numerics;

using UnityEngine;

namespace SpaceAce.Auxiliary
{
    public static class AuxMath
    {
        private static readonly HashAlgorithm _hashAlgorithm = SHA256.Create();
        private static readonly UTF8Encoding _utf8 = new(true, true);
        private static readonly System.Random _random = new();
        private static readonly HashSet<int> _firstPrimes = new() { 2, 3, 5, 7, 11, 13, 17, 19 };

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
            if (availableNumbers.Count() < amount)
                throw new ArgumentOutOfRangeException();

            HashSet<int> numbersToUse = new(availableNumbers);
            List<int> generatedNumbers = new(amount);

            for (int i = 0; i < amount; i++)
            {
                int index = UnityEngine.Random.Range(0, numbersToUse.Count);
                int randomNumberFromRange = numbersToUse.ElementAt(index);

                generatedNumbers.Add(randomNumberFromRange);
                numbersToUse.Remove(index);
            }

            return generatedNumbers;
        }

        public static bool ValueInRange(UnityEngine.Vector2 range, float value) => value >= range.x && value <= range.y;

        public static bool ValueInRange(float min, float max, float value) => value >= min && value <= max;

        public static Dictionary<UnityEngine.Vector2, T> InterpolateEnumByRange<T>(AnimationCurve interpolation, IEnumerable<T> customOrder = null) where T : Enum
        {
            if (interpolation == null)
                throw new ArgumentNullException();

            T[] members;

            if (customOrder is null) members = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            else members = customOrder.ToArray();

            Dictionary<UnityEngine.Vector2, T> result = new(members.Length);

            for (int i = 0; i < members.Length; i++)
            {
                float rangeStart = interpolation.Evaluate((float)i / members.Length);
                float rangeEnd = interpolation.Evaluate((float)(i + 1) / members.Length);

                UnityEngine.Vector2 range = new(rangeStart, rangeEnd);
                T member = members[i];

                result.Add(range, member);
            }

            return result;
        }

        public static Dictionary<UnityEngine.Vector2, T> InterpolateValuesByRange<T>(AnimationCurve interpolation, IEnumerable<T> values)
        {
            if (interpolation == null)
                throw new ArgumentNullException();

            if (values is null)
                throw new ArgumentNullException();

            int valuesAmount = values.Count();
            int counter = 0;

            Dictionary<UnityEngine.Vector2, T> result = new(valuesAmount);

            foreach (T value in values)
            {
                float rangeStart = interpolation.Evaluate((float)counter / valuesAmount);
                float rangeEnd = interpolation.Evaluate((float)(counter + 1) / valuesAmount);

                UnityEngine.Vector2 range = new(rangeStart, rangeEnd);
                result.Add(range, value);

                counter++;
            }

            return result;
        }

        public static Dictionary<T, UnityEngine.Vector2> InterpolateRangeByEnum<T>(AnimationCurve interpolation, IEnumerable<T> customOrder = null) where T : Enum
        {
            if (interpolation == null) throw new ArgumentNullException();

            T[] members;

            if (customOrder is null)
                members = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            else
                members = customOrder.ToArray();

            Dictionary<T, UnityEngine.Vector2> result = new(members.Length);

            for (int i = 0; i < members.Length; i++)
            {
                float rangeStart = interpolation.Evaluate((float)i / members.Length);
                float rangeEnd = interpolation.Evaluate((float)(i + 1) / members.Length);

                UnityEngine.Vector2 range = new(rangeStart, rangeEnd);
                T member = members[i];

                result.Add(member, range);
            }

            return result;
        }

        public static void PrimeTransform(byte[] buffer)
        {
            if (buffer is null)
                throw new ArgumentNullException();

            if (buffer.Length <= 2)
                throw new Exception("The buffer must be at least 3 bytes long!");

            int period = NearestPrimeBelow(buffer.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                int target = (i + period) % buffer.Length;
                int supplementor = (i + period - 1) % buffer.Length;

                buffer[target] = IsPrime(i) ? (byte)(buffer[i] - buffer[supplementor])
                                            : (byte)(buffer[i] + buffer[supplementor]);
            }
        }

        public static int NearestPrimeAbove(int n)
        {
            if (n < 2) return 2;

            int smallestCandidate = n % 2 == 0 ? n + 1 : n + 2;

            for (int i = smallestCandidate; ; i += 2)
                if (IsPrime(i) == true)
                    return i;
        }

        public static int NearestPrimeBelow(int n)
        {
            if (n <= 2) throw new ArgumentOutOfRangeException();
            if (n == 3) return 2;

            int largestCandidate = n % 2 == 0 ? n - 1 : n - 2;

            for (int i = largestCandidate; i >= 3; i -= 2)
                if (IsPrime(i) == true)
                    return i;

            throw new ArithmeticException();
        }

        public static bool IsPrime(int candidate, float error = 1e-6f)
        {
            if (error <= 0f || error >= 1f)
                throw new ArgumentOutOfRangeException();

            if (candidate < 2) return false;

            if (_firstPrimes.Contains(candidate) == true)
                return true;

            foreach (int p in _firstPrimes)
                if (candidate % p == 0)
                    return false;

            int iterations = Mathf.CeilToInt(Mathf.Log(1f / error) / Mathf.Log(2f));

            for (int i = 0; i < iterations; i++)
            {
                int witness = UnityEngine.Random.Range(2, candidate);

                if (BigInteger.GreatestCommonDivisor(witness, candidate) != 1)
                    return false;

                if (BigInteger.ModPow(witness, candidate - 1, candidate) != 1)
                    return false;
            }

            return true;
        }

        public static void XORInternal(byte[] buffer, byte[] key)
        {
            if (buffer is null || key is null)
                throw new ArgumentNullException();

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = (byte)(buffer[i] ^ key[i % buffer.Length]);
        }

        public static byte[] XOR(byte[] data, byte[] key)
        {
            if (data is null || key is null)
                throw new ArgumentNullException();

            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ key[i % data.Length]);

            return result;
        }
    }
}