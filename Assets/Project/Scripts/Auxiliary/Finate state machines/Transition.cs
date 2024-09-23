using System;

namespace SpaceAce.Auxiliary.FinateStateMachines
{
    public sealed class Transition : IEquatable<Transition>
    {
        public static Transition Default => new(null, () => false);

        public IState To { get; }
        public Func<bool> Condition { get; }

        public Transition(IState to, Func<bool> condition)
        {
            To = to ?? throw new ArgumentNullException();
            Condition = condition ?? throw new ArgumentNullException();
        }

        public override bool Equals(object obj) => Equals(obj as Transition);

        public bool Equals(Transition other) => other is not null &&
                                                To.Equals(other.To) == true &&
                                                Condition == other.Condition;

        public static bool operator ==(Transition x, Transition y)
        {
            if (x is null)
            {
                if (y is null)
                {
                    return true;
                }

                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(Transition x, Transition y) => !(x == y);

        public override int GetHashCode() => To.GetHashCode() ^ Condition.GetHashCode();
    }
}