using System.Collections.Generic;
using System.Text;
using System;

using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToPlayerPrefsSavingSystem : ISavingSystem
    {
        public event EventHandler<SuccessEventArgs> SavingCompleted, LoadingCompleted;
        public event EventHandler<FailEventArgs> SavingFailed, LoadingFailed;

        private readonly UTF8Encoding _UTF8 = new(true, true);
        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator;
        private readonly IEncryptor _encryptor;

        public ToPlayerPrefsSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor)
        {
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException();
            _encryptor = encryptor ?? throw new ArgumentNullException();
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException();

            if (_savableEntities.Add(entity) == true)
            {
                entity.SavingRequested += (sender, args) => SaveStateToPlayerPrefs(entity);
                LoadStateFromPlayerPrefs(entity);
            }
        }

        public void Deregister(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException();

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
                byte[] encryptedState = _encryptor.Encrypt(byteState, key);
                string encryptedStateAsUTF8 = _UTF8.GetString(encryptedState);

                PlayerPrefs.SetString(entity.ID, encryptedStateAsUTF8);

                SavingCompleted?.Invoke(this, new(entity.ID));
            }
            catch (Exception ex)
            {
                SavingFailed?.Invoke(this, new(entity.ID, ex.Message));
            }
        }

        private void LoadStateFromPlayerPrefs(ISavable entity)
        {
            if (PlayerPrefs.HasKey(entity.ID) == true)
            {
                try
                {
                    string savedState = PlayerPrefs.GetString(entity.ID, string.Empty);
                    byte[] encryptedState = _UTF8.GetBytes(savedState);

                    byte[] key = _keyGenerator.GenerateKey(entity.ID);
                    byte[] decryptedState = _encryptor.Decrypt(encryptedState, key);

                    string state = _UTF8.GetString(decryptedState);
                    entity.SetState(state);

                    LoadingCompleted?.Invoke(this, new(entity.ID));
                }
                catch (Exception ex)
                {
                    LoadingFailed?.Invoke(this, new(entity.ID, ex.Message));
                }
            }
        }
    }
}