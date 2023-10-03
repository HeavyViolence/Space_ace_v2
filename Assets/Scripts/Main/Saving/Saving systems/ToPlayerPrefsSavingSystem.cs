using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToPlayerPrefsSavingSystem : ISavingSystem
    {
        private readonly HashSet<ISavable> _savableEntities = new();

        private readonly IKeyGenerator _keyGenerator = null;
        private readonly KeyStrength _keyStrength = KeyStrength.Default;
        private readonly IEncryptor _encryptor = null;

        public ToPlayerPrefsSavingSystem(IKeyGenerator keyGenerator, KeyStrength strength, IEncryptor encryptor)
        {
            if (keyGenerator is null) throw new ArgumentNullException(nameof(keyGenerator), $"Attempted to pass an empty {typeof(IKeyGenerator)}!");
            if (encryptor is null) throw new ArgumentNullException(nameof(encryptor), $"Attempted to pass an empty {typeof(IEncryptor)}!");

            _keyGenerator = keyGenerator;
            _keyStrength = strength;
            _encryptor = encryptor;
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), $"Attempted to pass an empty {typeof(ISavable)}!");

            if (_savableEntities.Add(entity) == true)
            {
                entity.SavingRequested += (sender, args) => SaveStateToPlayerPrefs(entity);

                if (PlayerPrefs.HasKey(entity.ID) == true)
                {
                    try
                    {
                        SetState(entity);
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
                entity.SavingRequested -= (sender, args) => SaveStateToPlayerPrefs(entity);
                _savableEntities.Remove(entity);

                try
                {
                    SaveStateToPlayerPrefs(entity);
                }
                catch (Exception)
                {

                }
            }
        }

        public void DeleteAllSaves() => PlayerPrefs.DeleteAll();

        private void SetState(ISavable entity)
        {
            string stateAsBase64 = PlayerPrefs.GetString(entity.ID, string.Empty);
            byte[] encryptedData = Convert.FromBase64String(stateAsBase64);

            int keySeed = entity.ID.GetHashCode();
            byte[] key = _keyGenerator.GenerateKey(_keyStrength, keySeed);

            byte[] decryptedData = _encryptor.Decrypt(encryptedData, key);

            UTF8Encoding utf8 = new(true, true);
            string state = utf8.GetString(decryptedData);

            entity.SetState(state);
        }

        private void SaveStateToPlayerPrefs(ISavable entity)
        {
            string state = entity.GetState();

            UTF8Encoding utf8 = new(true, true);
            byte[] data = utf8.GetBytes(state);

            int keySeed = entity.ID.GetHashCode();
            byte[] key = _keyGenerator.GenerateKey(_keyStrength, keySeed);

            byte[] encryptedData = _encryptor.Encrypt(data, key);
            string stateAsBase64 = Convert.ToBase64String(encryptedData);

            PlayerPrefs.SetString(entity.ID, stateAsBase64);
            PlayerPrefs.Save();
        }
    }
}