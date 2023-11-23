using Cysharp.Threading.Tasks;

namespace SpaceAce.UI
{
    public interface IScreenFader
    {
        UniTask FadeInAndOutAsync(float duration);
    }
}