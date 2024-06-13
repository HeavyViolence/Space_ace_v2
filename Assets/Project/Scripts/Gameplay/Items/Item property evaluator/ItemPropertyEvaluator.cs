using SpaceAce.Auxiliary;

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

        private readonly ItemPropertyEvaluatorConfig _config;

        public ItemPropertyEvaluator(ItemPropertyEvaluatorConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _goodPropertyInterpolators = BuildPropertyInterpolation(config.GoodPropertyInterpolationCurve);
            _badPropertyInterpolators = BuildPropertyInterpolation(config.BadPropertyInterpolationCurve);
            _priceInterpolators = BuildPropertyInterpolation(config.PriceInterpolationCurve);
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

        public float Evaluate(Vector2 range, bool goodProperty, Quality quality, Size size, SizeInfluence sizeInfluence)
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

            float evaluator = _config.ValueProbabilityCurvePerRange.Evaluate(AuxMath.RandomNormal);
            float value = Mathf.Lerp(minValue, maxValue, evaluator);

            switch (size)
            {
                case Size.Small:
                    {
                        return sizeInfluence switch
                        {
                            SizeInfluence.Direct => value * _config.SmallSizePropertyFactor,
                            SizeInfluence.Inverted => value / _config.SmallSizePropertyFactor,
                            SizeInfluence.None => value,
                            _ => value
                        };
                    }
                case Size.Medium: return value;
                case Size.Large:
                    {
                        return sizeInfluence switch
                        {
                            SizeInfluence.Direct => value * _config.LargeSizePropertyFactor,
                            SizeInfluence.Inverted => value / _config.LargeSizePropertyFactor,
                            SizeInfluence.None => value,
                            _ => value
                        };
                    }
                default: return value;
            }
        }

        public int Evaluate(Vector2Int range, bool goodProperty, Quality quality, Size size, SizeInfluence sizeInfluence)
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

            float evaluator = _config.ValueProbabilityCurvePerRange.Evaluate(AuxMath.RandomNormal);
            float value = Mathf.Lerp(minValue, maxValue, evaluator);

            switch (size)
            {
                case Size.Small:
                    {
                        return sizeInfluence switch
                        {
                            SizeInfluence.Direct => (int)(value * _config.SmallSizePropertyFactor),
                            SizeInfluence.Inverted => (int)(value / _config.SmallSizePropertyFactor),
                            SizeInfluence.None => (int)value,
                            _ => (int)value
                        };
                    }
                case Size.Medium: return (int)value;
                case Size.Large:
                    {
                        return sizeInfluence switch
                        {
                            SizeInfluence.Direct => (int)(value * _config.LargeSizePropertyFactor),
                            SizeInfluence.Inverted => (int)(value / _config.LargeSizePropertyFactor),
                            SizeInfluence.None => (int)value,
                            _ => (int)value
                        };
                    }
                default: return (int)value;
            }
        }

        public float EvaluatePrice(Vector2 priceRange, Quality quality, Size size)
        {
            Vector2 rangeInterpolators = _priceInterpolators[quality];

            float minPrice = Mathf.Lerp(priceRange.x, priceRange.y, rangeInterpolators.x);
            float maxPrice = Mathf.Lerp(priceRange.x, priceRange.y, rangeInterpolators.y);

            float evaluator = _config.ValueProbabilityCurvePerRange.Evaluate(AuxMath.RandomNormal);
            float price = Mathf.Lerp(minPrice, maxPrice, evaluator);

            return size switch
            {
                Size.Small => price * _config.SmallSizePropertyFactor,
                Size.Medium => price,
                Size.Large => price * _config.LargeSizePropertyFactor,
                _ => price
            };
        }
    }
}