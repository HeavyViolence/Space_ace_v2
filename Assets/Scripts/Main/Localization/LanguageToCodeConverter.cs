namespace SpaceAce.Main.Localization
{
    public sealed class LanguageToCodeConverter
    {
        public LanguageToCodeConverter() { }

        public string GetLanguageCode(Language language)
        {
            string languageCode = language switch
            {
                Language.EnglishUnitedStates => "en-US",
                Language.Russian => "ru-RU",
                _ => "en-US"
            };

            return languageCode;
        }
    }
}