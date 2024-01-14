using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemPropertyEvaluator
    {
        private readonly Dictionary<Quality, Vector2> _goodPropertyInterpolators;
        private readonly Dictionary<Quality, Vector2> _badPropertyInterpolators;
        private readonly Dictionary<Quality, Vector2> _priceInterpolators;

        private readonly float _smallSizePropertyFactor;
        private readonly float _largeSizePropertyFactor;

        public ItemPropertyEvaluator(ItemPropertyEvaluatorConfig config)
        {
            if (config == null) throw new ArgumentNullException();

            _goodPropertyInterpolators = BuildPropertyInterpolation(config.GoodPropertyInterpolationCurve);
            _badPropertyInterpolators = BuildPropertyInterpolation(config.BadPropertyInterpolationCurve);
            _priceInterpolators = BuildPropertyInterpolation(config.PriceInterpolationCurve);

            _smallSizePropertyFactor = config.SmallSizePropertyFactor;
            _largeSizePropertyFactor = config.LargeSizePropertyFactor;
        }

        private Dictionary<Quality, Vector2> BuildPropertyInterpolation(AnimationCurve interpolationCurve)
        {
            int evaluatorsAmount = Enum.GetValues(typeof(Quality)).Length;
            string[] itemQualities = Enum.GetNames(typeof(Quality));

            Dictionary<Quality, Vector2> cache = new(evaluatorsAmount);

            for (int i = 0; i < evaluatorsAmount; i++)
            {
                float rangeStartEvaluator = (float)i / evaluatorsAmount;
                float rangeEndEvaluator = (float)(i + 1) / evaluatorsAmount;

                float rangeStartInterpolator = interpolationCurve.Evaluate(rangeStartEvaluator);
                float rangeEndInterpolator = interpolationCurve.Evaluate(rangeEndEvaluator);

                Quality quality = Enum.Parse<Quality>(itemQualities[i], true);

                cache.Add(quality, new(rangeStartInterpolator, rangeEndInterpolator));
            }

            return cache;
        }

        public float Evaluate(Vector2 range, bool goodProperty, Quality quality, Size size)
        {
            float minValue, maxValue;

            if (goodProperty == true)
            {
                Vector2 rangeInterpolators = _goodPropertyInterpolators[quality];

                minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
            }
            else
            {
                Vector2 rangeInterpolators = _badPropertyInterpolators[quality];

                minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
                maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
            }

            float delta = (maxValue - minValue) / Mathf.Sqrt(5);
            float value = UnityEngine.Random.Range(minValue + delta, maxValue - delta);

            return size switch
            {
                Size.Small => value * _smallSizePropertyFactor,
                Size.Medium => value,
                Size.Large => value * _largeSizePropertyFactor,
                _ => value
            };
        }

        public int Evaluate(Vector2Int range, bool goodProperty, Quality quality, Size size)
        {
            float minValue, maxValue;

            if (goodProperty == true)
            {
                Vector2 rangeInterpolators = _goodPropertyInterpolators[quality];

                minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
            }
            else
            {
                Vector2 rangeInterpolators = _badPropertyInterpolators[quality];

                minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
                maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
            }

            float delta = (maxValue - minValue) / Mathf.Sqrt(5);
            float value = UnityEngine.Random.Range(minValue + delta, maxValue - delta);

            return size switch
            {
                Size.Small => (int)(value * _smallSizePropertyFactor),
                Size.Medium => (int)value,
                Size.Large => (int)(value * _largeSizePropertyFactor),
                _ => (int)value
            };
        }

        public float EvaluatePrice(Vector2 priceRange, Quality quality, Size size)
        {
            Vector2 rangeInterpolators = _priceInterpolators[quality];

            float minPrice = Mathf.Lerp(priceRange.x, priceRange.y, rangeInterpolators.x);
            float maxPrice = Mathf.Lerp(priceRange.x, priceRange.y, rangeInterpolators.y);
            float delta = (maxPrice - minPrice) / Mathf.Sqrt(5);
            float price = UnityEngine.Random.Range(minPrice + delta, maxPrice - delta);

            return size switch
            {
                Size.Small => price * _smallSizePropertyFactor,
                Size.Medium => price,
                Size.Large => price * _largeSizePropertyFactor,
                _ => price
            };
        }
    }
}