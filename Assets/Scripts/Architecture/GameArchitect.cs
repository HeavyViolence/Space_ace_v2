using NaughtyAttributes;
using SpaceAce.Gameplay.Players;
using SpaceAce.Main;
using SpaceAce.Main.Audio;
using SpaceAce.Main.ObjectPooling;
using SpaceAce.Main.Saving;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SpaceAce.Architecture
{
    public sealed class GameArchitect : MonoBehaviour
    {
        private const string SpaceBackgroundFoldoutName = "Space background";
        private const string CameraFoldoutName = "Camera";
        private const string SavingSystemFoldoutName = "Saving system";
        private const string AudioFoldoutName = "Audio";

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
        private SavingSystemType _savingSystemType = SavingSystemType.ToFile;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private KeyGeneratorType _keyGeneratorType = KeyGeneratorType.RandomWithSeed;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private EncryptionType _encryptionType = EncryptionType.AES;

        [SerializeField, Foldout(SavingSystemFoldoutName)]
        private KeyStrength _keyStrength = KeyStrength.Default;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioMixer _audioMixer;

        [SerializeField, Foldout(AudioFoldoutName)]
        private AudioCollection _music;

        #endregion

        private IEnumerable<IUpdatable> _updatableServices = null;
        private IEnumerable<IFixedUpdatable> _fixedUpdatableServices = null;

        private void Awake()
        {
            InstantiateServices();
            CacheUpdatableServices();
            CacheFixedUpdatableServices();
            InitializeServices();
        }

        private void OnDestroy() => DisposeServices();

        private void InstantiateServices()
        {
            GameObject spaceBackgroundObject = Instantiate(_spaceBackgroundPrefab);

            GamePauser gamePauser = new();
            Services.Register(gamePauser);

            LevelLoader levelLoader = new();
            Services.Register(levelLoader);

            MainMenuLoader mainMenuLoader = new(levelLoader);
            Services.Register(mainMenuLoader);

            SpaceBackground spaceBackground = new(spaceBackgroundObject,
                                                  _mainMenuSpaceBackground,
                                                  _levelsSpaceBackgrounds,
                                                  gamePauser,
                                                  levelLoader,
                                                  mainMenuLoader);
            Services.Register(spaceBackground);

            GameObject masterCameraObject = Instantiate(_cameraPrefab);

            MasterCameraHolder masterCameraHolder = new(masterCameraObject);
            Services.Register(masterCameraHolder);

            ISavingSystem savingSystem = InstantiateSavingSystem();
            Services.Register(savingSystem);

            Player player = new(savingSystem);
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
        }

        private ISavingSystem InstantiateSavingSystem()
        {
            IKeyGenerator keyGenerator = _keyGeneratorType switch
            {
                KeyGeneratorType.Blank => new BlankKeyGenerator(),
                KeyGeneratorType.RandomWithSeed => new RandomKeyGenerator(),
                KeyGeneratorType.RandomCryptosafe => new CryptosafeKeyGenerator(),
                _ => new BlankKeyGenerator(),
            };

            IEncryptor encryptor = _encryptionType switch
            {
                EncryptionType.None => new BlankEncryptor(),
                EncryptionType.XOR => new XOREncryptor(),
                EncryptionType.AES => new AESEncryptor(),
                _ => new BlankEncryptor(),
            };

            ISavingSystem savingSystem = _savingSystemType switch
            {
                SavingSystemType.ToPlayerPrefs => new ToPlayerPrefsSavingSystem(keyGenerator, _keyStrength, encryptor),
                SavingSystemType.ToFile => new ToFileSavingSystem(keyGenerator, _keyStrength, encryptor, Application.persistentDataPath),
                _ => new ToFileSavingSystem(keyGenerator, _keyStrength, encryptor, Application.persistentDataPath),
            };

            return savingSystem;
        }

        private void InitializeServices()
        {
            if (Services.TryGet(out IEnumerable<IInitializable> services) == true)
                foreach (var service in services)
                    service.Initialize();
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