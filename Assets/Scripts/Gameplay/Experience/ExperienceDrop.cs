using System;

namespace SpaceAce.Gameplay.Experience
{
    public readonly struct ExperienceDrop
    {
        public float Gain { get; }
        public float Loss { get; }

        public float Total => Gain + Loss;
        public float Efficiency => Gain / Total;

        public ExperienceDrop(float gain, float loss)
        {
            if (gain < 0f) throw new ArgumentOutOfRangeException();
            Gain = gain;

            if (loss < 0f) throw new ArgumentOutOfRangeException();
            Loss = loss;
        }

        #region static

        public static ExperienceDrop operator +(ExperienceDrop left, ExperienceDrop right) =>
            new(left.Gain + right.Gain, left.Loss + right.Loss);

        public static ExperienceDrop None => new(0f, 0f);

        #endregion
    }
}