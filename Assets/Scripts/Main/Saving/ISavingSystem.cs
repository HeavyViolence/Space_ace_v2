namespace SpaceAce.Main.Saving
{
    public interface ISavingSystem
    {
        void Register(ISavable entity);
        void Deregister(ISavable entity);
        void DeleteAllSaves();
    }
}