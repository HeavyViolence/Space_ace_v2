using SpaceAce.Auxiliary;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Items
{
    public sealed class ItemPropertyEvaluator
    {
        private readonly Dictionary<Quality, Vector2> _goodPropertyInterpolation;
        private readonly Dictionary<Quality, Vector2> _badPropertyInterpolation;
        private readonly Dictionary<Quality, Vector2> _priceInterpolation;

        private readonly ItemPropertyEvaluatorConfig _config;

        public ItemPropertyEvaluator(ItemPropertyEvaluatorConfig config)
        {
            if (config == null) throw new ArgumentNullException();
            _config = config;

            _goodPropertyInterpolation = AuxMath.InterpolateRangeByEnum<Quality>(config.GoodPropertyInterpolationCurve);
            _badPropertyInterpolation = AuxMath.InterpolateRangeByEnum<Quality>(config.BadPropertyInterpolationCurve);
            _priceInterpolation = AuxMath.InterpolateRangeByEnum<Quality>(config.PriceInterpolationCurve);
        }

        public float Evaluate(Vector2 range, RangeEvaluationDirection direction, Quality quality, Size size, SizeInfluence sizeInfluence)
        {
            float minValue, maxValue;

            switch (direction)
            {
                case RangeEvaluationDirection.Forward:
                    {
                        Vector2 rangeInterpolators = _goodPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);

                        break;
                    }
                case RangeEvaluationDirection.Backward:
                    {
                        Vector2 rangeInterpolators = _badPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);

                        break;
                    }
                default:
                    {
                        Vector2 rangeInterpolators = _goodPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);

                        break;
                    }
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

        public int Evaluate(Vector2Int range, RangeEvaluationDirection direction, Quality quality, Size size, SizeInfluence sizeInfluence)
        {
            float minValue, maxValue;

            switch (direction)
            {
                case RangeEvaluationDirection.Forward:
                    {
                        Vector2 rangeInterpolators = _goodPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);

                        break;
                    }
                case RangeEvaluationDirection.Backward:
                    {
                        Vector2 rangeInterpolators = _badPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);

                        break;
                    }
                default:
                    {
                        Vector2 rangeInterpolators = _goodPropertyInterpolation[quality];

                        minValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.x);
                        maxValue = Mathf.Lerp(range.x, range.y, rangeInterpolators.y);

                        break;
                    }
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
            Vector2 rangeInterpolators = _priceInterpolation[quality];

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