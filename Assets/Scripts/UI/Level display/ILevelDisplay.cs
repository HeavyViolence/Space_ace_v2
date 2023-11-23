using Cysharp.Threading.Tasks;

namespace SpaceAce.UI
{
    public interface ILevelDisplay
    {
        bool Enabled { get; }

        UniTask EnableAsync();
        void Disable();
    }
}