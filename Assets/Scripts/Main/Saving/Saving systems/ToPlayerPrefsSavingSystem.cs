using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToPlayerPrefsSavingSystem : ISavingSystem
    {
        private static readonly UTF8Encoding s_UTF8 = new(true, true);

        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator = null;
        private readonly IEncryptor _encryptor = null;

        public ToPlayerPrefsSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor)
        {
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator),
                $"Attempted to pass an empty {typeof(IKeyGenerator)}!");

            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor),
                $"Attempted to pass an empty {typeof(IEncryptor)}!");
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to pass an empty {typeof(ISavable)}!");

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
            if (entity is null) throw new ArgumentNullException(nameof(entity),
                $"Attempted to pass an empty {typeof(ISavable)}!");

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
            string encryptedByteStateAsBase64 = PlayerPrefs.GetString(entity.ID, string.Empty);
            byte[] encryptedByteState = Convert.FromBase64String(encryptedByteStateAsBase64);

            byte[] key = _keyGenerator.GenerateKey(entity);
            byte[] decryptedByteState = _encryptor.Decrypt(encryptedByteState, key);

            string state = s_UTF8.GetString(decryptedByteState);
            entity.SetState(state);
        }

        private void SaveStateToPlayerPrefs(ISavable entity)
        {
            string state = entity.GetState();
            byte[] byteState = s_UTF8.GetBytes(state);

            byte[] key = _keyGenerator.GenerateKey(entity);
            byte[] encryptedByteState = _encryptor.Encrypt(byteState, key);

            string encryptedByteStateAsBase64 = Convert.ToBase64String(encryptedByteState);

            PlayerPrefs.SetString(entity.ID, encryptedByteStateAsBase64);
            PlayerPrefs.Save();
        }
    }
}