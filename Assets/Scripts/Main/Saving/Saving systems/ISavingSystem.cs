using System;

namespace SpaceAce.Main.Saving
{
    public interface ISavingSystem
    {
        public event EventHandler SavingCompleted;
        public event EventHandler SavingFailed;

        public event EventHandler LoadingCompleted;
        public event EventHandler LoadingFailed;

        void Register(ISavable entity);
        void Deregister(ISavable entity);
        void DeleteAllSaves();
    }
}