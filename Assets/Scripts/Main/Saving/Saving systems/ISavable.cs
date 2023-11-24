using System;

namespace SpaceAce.Main.Saving
{
    public interface ISavable : IEquatable<ISavable>
    {
        event EventHandler SavingRequested;

        string ID { get; }

        string GetState();
        void SetState(string state);
    }
}