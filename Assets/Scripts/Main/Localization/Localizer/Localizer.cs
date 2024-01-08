using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using SpaceAce.Main.Saving;

using System;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

using Zenject;

namespace SpaceAce.Main.Localization
{
    public sealed class Localizer : IInitializable, IDisposable, ISavable
    {
        public event EventHandler SavingRequested;

        private readonly LocalizedFont _localizedFont;
        private readonly LanguageToCodeConverter _languageToCodeConverter;
        private readonly ISavingSystem _savingSystem;

        private bool _initialized = false;

        public Language ActiveLanguage { get; private set; } = Language.EnglishUnitedStates;
        public string ID => "Localization";

        public Localizer(LocalizedFont localizedFont,
                         LanguageToCodeConverter languageToCodeConverter,
                         ISavingSystem savingSystem)
        {
            _localizedFont = localizedFont ?? throw new ArgumentNullException();
            _languageToCodeConverter = languageToCodeConverter ?? throw new ArgumentNullException();
            _savingSystem = savingSystem ?? throw new ArgumentNullException();
        }

        public async UniTask<string> GetLocalizedStringAsync(string tableName, string entryName, params object[] arguments)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(entryName) || string.IsNullOrWhiteSpace(entryName))
                throw new ArgumentNullException();

            LocalizedString localizedString = new(tableName, entryName) { Arguments = arguments };

            var operation = localizedString.GetLocalizedStringAsync();
            await UniTask.WaitUntil(() => operation.IsDone);

            return operation.Result;
        }

        public async UniTask<Font> GetLocalizedFontAsync()
        {
            var operation = _localizedFont.LoadAssetAsync();
            await UniTask.WaitUntil(() => operation.IsDone);

            return operation.Result;
        }

        public async UniTask SetLanguageAsync(Language language)
        {
            if (_initialized == true && language == ActiveLanguage) return;

            var operation = LocalizationSettings.InitializationOperation;
            await UniTask.WaitUntil(() => operation.IsDone);

            string activeLanguageCode = _languageToCodeConverter.GetLanguageCode(language);
            Locale activeLocale = null;

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                if (locale.Identifier.Code == activeLanguageCode)
                    activeLocale = locale;

            Language previouslySelectedLanguage = ActiveLanguage;

            LocalizationSettings.SelectedLocale = activeLocale;
            ActiveLanguage = language;

            if (previouslySelectedLanguage != language)
                SavingRequested?.Invoke(this, EventArgs.Empty);

            _initialized = true;
        }

        #region interfaces

        public void Initialize()
        {
            _savingSystem.Register(this);
            SetLanguageAsync(ActiveLanguage).Forget();
        }

        public void Dispose()
        {
            _savingSystem.Deregister(this);
        }

        public string GetState() => JsonConvert.SerializeObject(ActiveLanguage);

        public void SetState(string state)
        {
            try
            {
                Language selectedLanguage = JsonConvert.DeserializeObject<Language>(state);
                ActiveLanguage = selectedLanguage;
            }
            catch (Exception)
            {
                ActiveLanguage = Language.EnglishUnitedStates;
            }
        }

        public override bool Equals(object obj) => obj is not null && Equals(obj as ISavable);

        public bool Equals(ISavable other) => other is not null && ID == other.ID;

        public override int GetHashCode() => ID.GetHashCode();

        #endregion
    }
}