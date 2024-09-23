namespace SpaceAce.Main.Saving
{
    public interface IKeyValidator
    {
        bool IsKeyValid(byte[] key);
    }
}