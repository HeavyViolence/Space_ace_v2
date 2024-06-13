using System;

namespace SpaceAce.Main.Saving
{
    public interface ISavable : IEquatable<ISavable>
    {
        event EventHandler SavingRequested;

        string SavedDataName { get; }

        string GetState();
        void SetState(string state);
    }
}