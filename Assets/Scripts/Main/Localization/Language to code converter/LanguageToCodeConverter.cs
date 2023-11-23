namespace SpaceAce.Main.Localization
{
    public sealed class LanguageToCodeConverter : ILanguageToCodeConverter
    {
        public string GetLanguageCode(Language language)
        {
            string code = language switch
            {
                Language.EnglishUnitedStates => "en-US",
                Language.Russian => "ru-RU",
                _ => "en-US"
            };

            return code;
        }
    }
}