namespace SpaceAce.Main.Localization
{
    public sealed class LanguageToCodeConverter
    {
        public string GetLanguageCode(Language language)
        {
            return language switch
            {
                Language.EnglishUnitedStates => "en-US",
                Language.Russian => "ru-RU",
                _ => "en-US"
            };
        }
    }
}