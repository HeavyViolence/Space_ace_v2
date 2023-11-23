using Cysharp.Threading.Tasks;

using UnityEngine;

namespace SpaceAce.Main.Localization
{
    public interface ILocalizer
    {
        Language ActiveLanguage { get; }

        UniTask<string> GetLocalizedStringAsync(string tableName, string entryName, params object[] arguments);
        UniTask<Font> GetLocalizedFontAsync();
        UniTask SetLanguageAsync(Language language);
    }
}