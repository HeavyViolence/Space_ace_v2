using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceAce.Main.Saving
{
    public sealed class ToFileSavingSystem : ISavingSystem
    {
        private const string SavesExtension = ".save";

        private readonly HashSet<ISavable> _savableEntities = new();

        private readonly IKeyGenerator _keyGenerator = null;
        private readonly KeyStrength _keyStrength = KeyStrength.Default;
        private readonly IEncryptor _encryptor = null;
        private readonly string _savesDirectory = string.Empty;

        public ToFileSavingSystem(IKeyGenerator keyGenerator, KeyStrength strength, IEncryptor encryptor, string savesDirectory)
        {
            if (keyGenerator is null) throw new ArgumentNullException(nameof(keyGenerator), $"Attempted to pass an empty {typeof(IKeyGenerator)}!");
            if (encryptor is null) throw new ArgumentNullException(nameof(encryptor), $"Attempted to pass an empty {typeof(IEncryptor)}!");
            if (string.IsNullOrEmpty(savesDirectory) || string.IsNullOrWhiteSpace(savesDirectory))
                throw new ArgumentNullException(nameof(savesDirectory), "Attempted to pass an empty saves directory path!");

            _keyGenerator = keyGenerator;
            _keyStrength = strength;
            _encryptor = encryptor;
            _savesDirectory = savesDirectory;
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Add(entity) == true)
            {
                entity.SavingRequested += (sender, args) => SaveStateToFile(entity);
                string saveFilePath = GetSaveFilePath(entity);

                if (File.Exists(saveFilePath) == true)
                {
                    try
                    {
                        SetState(entity, saveFilePath);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        public void Deregister(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Contains(entity) == true)
            {
                entity.SavingRequested -= (sender, args) => SaveStateToFile(entity);
                _savableEntities.Remove(entity);

                try
                {
                    SaveStateToFile(entity);
                }
                catch (Exception)
                {

                }
            }
        }

        public void DeleteAllSaves()
        {
            foreach (var entity in _savableEntities)
            {
                string saveFilePath = GetSaveFilePath(entity);

                if (File.Exists(saveFilePath) == true) File.Delete(saveFilePath);
            }
        }

        private void SetState(ISavable entity, string saveFilePath)
        {
            byte[] encryptedData = File.ReadAllBytes(saveFilePath);

            int keySeed = entity.ID.GetHashCode();
            byte[] key = _keyGenerator.GenerateKey(_keyStrength, keySeed);

            byte[] decryptedData = _encryptor.Decrypt(encryptedData, key);

            UTF8Encoding utf8 = new(true, true);
            string state = utf8.GetString(decryptedData);

            entity.SetState(state);
        }

        private void SaveStateToFile(ISavable entity)
        {
            string state = entity.GetState();

            UTF8Encoding utf8 = new(true, true);
            byte[] data = utf8.GetBytes(state);

            int keySeed = entity.ID.GetHashCode();
            byte[] key = _keyGenerator.GenerateKey(_keyStrength, keySeed);

            byte[] encryptedData = _encryptor.Encrypt(data, key);
            string saveFilePath = GetSaveFilePath(entity);

            File.WriteAllBytes(saveFilePath, encryptedData);
        }

        private string GetSaveFilePath(ISavable entity) => Path.Combine(_savesDirectory, entity.ID + SavesExtension);
    }
}