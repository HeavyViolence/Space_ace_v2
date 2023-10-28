using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToFileSavingSystem : ISavingSystem
    {
        private const string SavesExtension = ".save";

        private readonly UTF8Encoding _UTF8 = new(true, true);
        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator = null;
        private readonly IEncryptor _encryptor = null;

        public ToFileSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor)
        {
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator),
                $"Attempted to pass an empty {typeof(IKeyGenerator)}!");

            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor),
                $"Attempted to pass an empty {typeof(IEncryptor)}!");
        }

        public void Register(ISavable entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity),
                    $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Add(entity) == true)
            {
                entity.SavingRequested += (sender, args) => SaveStateToFile(entity);
                SetState(entity);
            }
        }

        public void Deregister(ISavable entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity),
                    $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Contains(entity) == true)
            {
                entity.SavingRequested -= (sender, args) => SaveStateToFile(entity);
                _savableEntities.Remove(entity);

                SaveStateToFile(entity);
            }
        }

        public void DeleteAllSaves()
        {
            foreach (var entity in _savableEntities)
            {
                string saveFilePath = GetSaveFilePath(entity.ID);
                if (File.Exists(saveFilePath) == true) File.Delete(saveFilePath);
            }
        }

        private void SetState(ISavable entity)
        {
            string saveFilePath = GetSaveFilePath(entity.ID);

            if (File.Exists(saveFilePath) == true)
            {
                try
                {
                    byte[] encryptedByteState = File.ReadAllBytes(saveFilePath);

                    byte[] key = _keyGenerator.GenerateKey(entity.ID);
                    byte[] decryptedByteState = _encryptor.Decrypt(encryptedByteState, key);

                    string state = _UTF8.GetString(decryptedByteState);
                    entity.SetState(state);
                }
                catch (Exception) { }
            }
        }

        private void SaveStateToFile(ISavable entity)
        {
            try
            {
                string state = entity.GetState();
                byte[] byteState = _UTF8.GetBytes(state);

                byte[] key = _keyGenerator.GenerateKey(entity.ID);
                byte[] encryptedByteState = _encryptor.Encrypt(byteState, key);

                string saveFilePath = GetSaveFilePath(entity.ID);
                File.WriteAllBytes(saveFilePath, encryptedByteState);
            }
            catch (Exception) { }
        }

        private string GetSaveFilePath(string id) => Path.Combine(Application.persistentDataPath, id + SavesExtension);
    }
}