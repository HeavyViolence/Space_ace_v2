using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Saving
{
    public sealed class SavingSystemInstaller : MonoInstaller
    {
        [SerializeField]
        private KeyGenerationType _keyGenerationType;

        [SerializeField]
        private EncryptionType _encryptionType;

        [SerializeField]
        private SavingSystemType _savingSystemType;

        public override void InstallBindings()
        {
            IKeyGenerator keyGenerator = _keyGenerationType switch
            {
                KeyGenerationType.Blank => new BlankKeyGenerator(),
                KeyGenerationType.Hash => new HashKeyGenerator(),
                _ => new BlankKeyGenerator()
            };

            IEncryptor encryptor = _encryptionType switch
            {
                EncryptionType.None => new BlankEncryptor(),
                EncryptionType.XOR => new XOREncryptor(),
                EncryptionType.RandomXOR => new RandomXOREncryptor(),
                EncryptionType.AES => new AESEncryptor(),
                _ => new BlankEncryptor()
            };

            ISavingSystem savingSystem = _savingSystemType switch
            {
                SavingSystemType.ToFile => new ToFileSavingSystem(keyGenerator, encryptor),
                SavingSystemType.ToPlayerPrefs => new ToPlayerPrefsSavingSystem(keyGenerator, encryptor),
                _ => new ToFileSavingSystem(keyGenerator, encryptor)
            };

            Container.BindInterfacesAndSelfTo<ISavingSystem>()
                     .FromInstance(savingSystem)
                     .AsSingle()
                     .NonLazy();
        }
    }
}