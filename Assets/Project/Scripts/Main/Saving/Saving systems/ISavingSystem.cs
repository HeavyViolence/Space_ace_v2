using System;

namespace SpaceAce.Main.Saving
{
    public interface ISavingSystem
    {
        public event EventHandler<SuccessEventArgs> SavingCompleted, LoadingCompleted;
        public event EventHandler<FailEventArgs> SavingFailed, LoadingFailed;

        void Register(ISavable entity);
        void Deregister(ISavable entity);
        void DeleteAllSaves();
    }
}