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

            Encryptor encryptor = _encryptionType switch
            {
                EncryptionType.None => new BlankEncryptor(new BlankKeyValidator()),
                EncryptionType.XOR => new XOREncryptor(new XORKeyValidator()),
                EncryptionType.RandomXOR => new RandomXOREncryptor(new XORKeyValidator()),
                EncryptionType.AES => new AESEncryptor(new AESKeyValidator()),
                _ => new BlankEncryptor(new BlankKeyValidator())
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