using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceAce.Main.Saving
{
    public sealed class ToFileSavingSystem : ISavingSystem
    {
        private const string SavesExtension = ".save";

        private static readonly UTF8Encoding s_UTF8 = new(true, true);

        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator = null;
        private readonly IEncryptor _encryptor = null;
        private readonly string _savesDirectory = string.Empty;

        public ToFileSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor, string savesDirectory)
        {
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator),
                $"Attempted to pass an empty {typeof(IKeyGenerator)}!");

            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor),
                $"Attempted to pass an empty {typeof(IEncryptor)}!");

            if (string.IsNullOrEmpty(savesDirectory) || string.IsNullOrWhiteSpace(savesDirectory))
                throw new ArgumentNullException(nameof(savesDirectory),
                    "Attempted to pass an empty saves directory path!");

            _savesDirectory = savesDirectory;
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to pass an empty {typeof(ISavable)}!");

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
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to pass an empty {typeof(ISavable)}!");

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
            byte[] encryptedByteState = File.ReadAllBytes(saveFilePath);

            byte[] key = _keyGenerator.GenerateKey(entity);
            byte[] decryptedByteState = _encryptor.Decrypt(encryptedByteState, key);

            string state = s_UTF8.GetString(decryptedByteState);
            entity.SetState(state);
        }

        private void SaveStateToFile(ISavable entity)
        {
            string state = entity.GetState();
            byte[] byteState = s_UTF8.GetBytes(state);

            byte[] key = _keyGenerator.GenerateKey(entity);
            byte[] encryptedByteState = _encryptor.Encrypt(byteState, key);

            string saveFilePath = GetSaveFilePath(entity);
            File.WriteAllBytes(saveFilePath, encryptedByteState);
        }

        private string GetSaveFilePath(ISavable entity) => Path.Combine(_savesDirectory, entity.ID + SavesExtension);
    }
}