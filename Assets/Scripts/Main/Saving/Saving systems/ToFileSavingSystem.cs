using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace SpaceAce.Main.Saving
{
    public sealed class ToFileSavingSystem : ISavingSystem
    {
        public event EventHandler<SuccessEventArgs> SavingCompleted, LoadingCompleted;
        public event EventHandler<FailEventArgs> SavingFailed, LoadingFailed;

        private const string SavesExtension = ".save";

        private readonly UTF8Encoding _UTF8 = new(true, true);
        private readonly HashSet<ISavable> _savableEntities = new();
        private readonly IKeyGenerator _keyGenerator;
        private readonly IEncryptor _encryptor;

        public ToFileSavingSystem(IKeyGenerator keyGenerator, IEncryptor encryptor)
        {
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException();
            _encryptor = encryptor ?? throw new ArgumentNullException();
        }

        public void Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException();

            if (_savableEntities.Add(entity) == true)
            {
                entity.SavingRequested += (sender, args) => SaveStateToFile(entity);
                LoadStateFromFile(entity);
            }
        }

        public void Deregister(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException();

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

        private void SaveStateToFile(ISavable entity)
        {
            try
            {
                string state = entity.GetState();
                byte[] byteState = _UTF8.GetBytes(state);

                byte[] key = _keyGenerator.GenerateKey(entity.ID);
                byte[] encryptedState = _encryptor.Encrypt(byteState, key);

                string saveFilePath = GetSaveFilePath(entity.ID);
                File.WriteAllBytes(saveFilePath, encryptedState);

                SavingCompleted?.Invoke(this, new(entity.ID));
            }
            catch (Exception ex)
            {
                SavingFailed?.Invoke(this, new(entity.ID, ex.Message));
            }
        }

        private void LoadStateFromFile(ISavable entity)
        {
            string saveFilePath = GetSaveFilePath(entity.ID);

            if (File.Exists(saveFilePath) == true)
            {
                try
                {
                    byte[] encryptedState = File.ReadAllBytes(saveFilePath);

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

        private string GetSaveFilePath(string id) => Path.Combine(Application.persistentDataPath, id + SavesExtension);
    }
}