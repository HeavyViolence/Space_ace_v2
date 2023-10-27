using NaughtyAttributes;

using SpaceAce.Gameplay.Levels;
using SpaceAce.Gameplay.Players;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.Localization;
using SpaceAce.Main.ObjectPooling;
using SpaceAce.Main.Saving;
using SpaceAce.UI;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace SpaceAce.Architecture
{
    public sealed class GameArchitect : MonoBehaviour
    {
        private const string SpaceBackgroundFoldoutName = "Space background";
        private const string CameraFoldoutName = "Camera";
        private const string SavingSystemFoldoutName = "Saving system";
        private const string AudioFoldoutName = "Audio";
        private const string UIFoldoutName = "UI";
        private const string LocalizationFoldoutName = "Localization";

        #region DI

        [SerializeField, Foldout(SpaceBackgroundFoldoutName)]
        private GameObject _spaceBackgroundPrefab;

        [SerializeField, Foldout(SpaceBackgroundFoldoutName)]
        private Material _mainMenuSpaceBackground;

        [SerializeField, Foldout(SpaceBackgroundFoldoutName)]
        private List<Material> _levelsSpaceBackgrounds;

        [SerializeField, Foldout(CameraFoldoutName)]
        private GameObject _cameraPrefab;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private SavingSystemType _savingSystemtype = SavingSystemType.ToFile;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private KeyGenerationType _keyGeneratorType = KeyGenerationType.Blank;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private EncryptionType _encryptionType = EncryptionType.None;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioMixer _audioMixer;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioCollection _music;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioCollection _levelCompletedAudio;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioCollection _levelFailedAudio;

        [SerializeField, Foldout(UIFoldoutName), Label("UI audio")]
        private UIAudio _uiAudio;

        [SerializeField, Foldout(UIFoldoutName)]
        private VisualTreeAsset _mainMenuAsset;

        [SerializeField, Foldout(UIFoldoutName)]
        private PanelSettings _mainMenuSettings;

        [SerializeField, Foldout(UIFoldoutName)]
        private VisualTreeAsset _levelSelectionMenuAsset;

        [SerializeField, Foldout(UIFoldoutName)]
        private VisualTreeAsset _levelButtonAsset;

        [SerializeField, Foldout(UIFoldoutName)]
        private PanelSettings _levelSelectionMenuSettings;

        [SerializeField, Foldout(UIFoldoutName)]
        private VisualTreeAsset _screenFaderAsset;

        [SerializeField, Foldout(UIFoldoutName)]
        private PanelSettings _screenFaderSettings;

        [SerializeField, Foldout(UIFoldoutName)]
        private AnimationCurve _fadingCurve;

        [SerializeField, Foldout(LocalizationFoldoutName)]
        private LocalizedFont _localizedFont;

        #endregion

        private IEnumerable<IUpdatable> _updatableServices = null;
        private IEnumerable<IFixedUpdatable> _fixedUpdatableServices = null;

        private void Awake()
        {
            InstantiateServices();
            CacheUpdatableServices();
            CacheFixedUpdatableServices();
            InitializeServices();
            RunServices();
        }

        private void OnDestroy()
        {
            StopServices();
            DisposeServices();
        }

        private void InstantiateServices()
        {
            GameObject spaceBackgroundObject = Instantiate(_spaceBackgroundPrefab);

            GamePauser gamePauser = new();
            Services.Register(gamePauser);

            LevelsLoader levelsLoader = new();
            Services.Register(levelsLoader);

            MainMenuLoader mainMenuLoader = new(levelsLoader);
            Services.Register(mainMenuLoader);

            SpaceBackground spaceBackground = new(spaceBackgroundObject,
                                                  _mainMenuSpaceBackground,
                                                  _levelsSpaceBackgrounds,
                                                  gamePauser,
                                                  levelsLoader,
                                                  mainMenuLoader);
            Services.Register(spaceBackground);

            GameObject masterCameraObject = Instantiate(_cameraPrefab);

            MasterCameraHolder masterCameraHolder = new(masterCameraObject);
            Services.Register(masterCameraHolder);

            ISavingSystem savingSystem = InstantiateSavingSystem();
            Services.Register(savingSystem);

            Player player = new(savingSystem, levelsLoader, mainMenuLoader);
            Services.Register(player);

            MasterAudioListenerHolder masterAudioListenerHolder = new(masterCameraObject, player, mainMenuLoader);
            Services.Register(masterAudioListenerHolder);

            MasterCameraShaker cameraShaker = new(masterCameraObject, gamePauser, savingSystem);
            Services.Register(cameraShaker);

            AudioPlayer audioPlayer = new(savingSystem, _audioMixer);
            Services.Register(audioPlayer);

            MusicPlayer musicPlayer = new(_music, savingSystem);
            Services.Register(musicPlayer);

            MultiobjectPool multiobjectPool = new(gamePauser);
            Services.Register(multiobjectPool);

            LevelsCompleter levelsCompleter = new(levelsLoader, player, _levelCompletedAudio, _levelFailedAudio);
            Services.Register(levelsCompleter);

            LevelsUnlocker levelsUnlocker = new(savingSystem, levelsCompleter);
            Services.Register(levelsUnlocker);

            LevelStopwatch levelStopwatch = new(levelsLoader, levelsCompleter, gamePauser);
            Services.Register(levelStopwatch);

            BestLevelsRunsStatisticsCollector bestLevelsRunsStatisticsColector = new(savingSystem, levelsCompleter, levelStopwatch, player);
            Services.Register(bestLevelsRunsStatisticsColector);

            LanguageToCodeConverter languageToCodeConverter = new();
            Services.Register(languageToCodeConverter);

            Localizer localizer = new(_localizedFont, languageToCodeConverter, savingSystem);
            Services.Register(localizer);

            MainMenuDisplay mainMenuDisplay = new(_mainMenuAsset, _mainMenuSettings, _uiAudio, localizer);
            Services.Register(mainMenuDisplay);

            LevelSelectionDisplay levelSelectionDisplay = new(_levelSelectionMenuAsset,
                                                              _levelButtonAsset,
                                                              _levelSelectionMenuSettings,
                                                              _uiAudio,
                                                              localizer,
                                                              levelsLoader,
                                                              levelsUnlocker,
                                                              bestLevelsRunsStatisticsColector);
            Services.Register(levelSelectionDisplay);

            ScreenFader screenFader = new(_screenFaderAsset,
                                          _screenFaderSettings,
                                          _uiAudio,
                                          localizer,
                                          mainMenuLoader,
                                          levelsLoader,
                                          _fadingCurve);
            Services.Register(screenFader);
        }

        private ISavingSystem InstantiateSavingSystem()
        {
            IKeyGenerator keyGenerator = _keyGeneratorType switch
            {
                KeyGenerationType.Blank => new BlankKeyGenerator(),
                KeyGenerationType.Hash => new HashKeyGenerator(),
                _ => new BlankKeyGenerator(),
            };

            IEncryptor encryptor = _encryptionType switch
            {
                EncryptionType.None => new BlankEncryptor(),
                EncryptionType.XOR => new XOREncryptor(),
                EncryptionType.RandomXOR => new RandomXOREncryptor(),
                EncryptionType.AES => new AESEncryptor(),
                _ => new BlankEncryptor(),
            };

            ISavingSystem savingSystem = _savingSystemtype switch
            {
                SavingSystemType.ToFile => new ToFileSavingSystem(keyGenerator, encryptor),
                _ => new ToFileSavingSystem(keyGenerator, encryptor)
            };

            return savingSystem;
        }

        private void InitializeServices()
        {
            if (Services.TryGet(out IEnumerable<IInitializable> services) == true)
                foreach (var service in services)
                    service.Initialize();
        }

        private void RunServices()
        {
            if (Services.TryGet(out IEnumerable<IRunnable> runnbaleServices) == true)
                foreach(var service in runnbaleServices)
                    service.Run();
        }

        private void StopServices()
        {
            if (Services.TryGet(out IEnumerable<IStoppable> stoppableServices) == true)
                foreach (var service in stoppableServices)
                    service.Stop();
        }

        private void CacheUpdatableServices()
        {
            if (Services.TryGet(out IEnumerable<IUpdatable> services) == true)
                _updatableServices = services;
        }

        private void CacheFixedUpdatableServices()
        {
            if (Services.TryGet(out IEnumerable<IFixedUpdatable> services) == true)
                _fixedUpdatableServices = services;
        }

        private void UpdateServices()
        {
            if (_updatableServices is not null)
                foreach (IUpdatable service in _updatableServices)
                    service.Update();
        }

        private void FixedUpdateServices()
        {
            if (_fixedUpdatableServices is not null)
                foreach (IFixedUpdatable service in _fixedUpdatableServices)
                    service.FixedUpdate();
        }

        private void Update() => UpdateServices();

        private void FixedUpdate() => FixedUpdateServices();

        private void DisposeServices()
        {
            if (Services.TryGet(out IEnumerable<IDisposable> services) == true)
                foreach (var service in services)
                    service.Dispose();

            Services.Clear();
        }
    }
}