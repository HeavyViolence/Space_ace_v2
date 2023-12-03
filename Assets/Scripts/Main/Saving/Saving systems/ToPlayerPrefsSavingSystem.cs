using System.Collections.Generic;
using System.Text;
using System;

using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToPlayerPrefsSavingSystem : ISavingSystem
    {
        private readonly UTF8Encoding _UTF8 = new(true, true);
        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator;
        private readonly IEncryptor _encryptor;

        public ToPlayerPrefsSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor)
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
                entity.SavingRequested += (sender, args) => SaveStateToPlayerPrefs(entity);
                LoadStateFromPlayerPrefs(entity);
            }
        }

        public void Deregister(ISavable entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity),
                    $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Contains(entity) == true)
            {
                entity.SavingRequested -= (sender, args) => SaveStateToPlayerPrefs(entity);
                _savableEntities.Remove(entity);

                SaveStateToPlayerPrefs(entity);
            }
        }

        public void DeleteAllSaves()
        {
            PlayerPrefs.DeleteAll();
        }

        private void SaveStateToPlayerPrefs(ISavable entity)
        {
            try
            {
                string state = entity.GetState();
                byte[] byteState = _UTF8.GetBytes(state);

                byte[] key = _keyGenerator.GenerateKey(entity.ID);
                byte[] encryptedByteState = _encryptor.Encrypt(byteState, key);
                string encryptedStateAsUTF8 = _UTF8.GetString(encryptedByteState);

                PlayerPrefs.SetString(entity.ID, encryptedStateAsUTF8);
            }
            catch (Exception) { }
        }

        private void LoadStateFromPlayerPrefs(ISavable entity)
        {
            if (PlayerPrefs.HasKey(entity.ID) == true)
            {
                try
                {
                    string savedState = PlayerPrefs.GetString(entity.ID, string.Empty);
                    byte[] encryptedByteState = _UTF8.GetBytes(savedState);

                    byte[] key = _keyGenerator.GenerateKey(entity.ID);
                    byte[] decryptedByteState = _encryptor.Decrypt(encryptedByteState, key);

                    string state = _UTF8.GetString(decryptedByteState);
                    entity.SetState(state);
                }
                catch (Exception) { }
            }
        }
    }
}