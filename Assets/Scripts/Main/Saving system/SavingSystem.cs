using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SpaceAce.Main
{
    public sealed class SavingSystem
    {
        private const string SavesDirectory = "D:/Unity Projects/Space_ace_v2/Saves";
        private const string SavesExtension = ".save";

        private const int KeyLength = 32;

        private readonly HashSet<ISavable> _registeredEntities = new();
        private readonly RandomNumberGenerator _randomKeyGenerator = RandomNumberGenerator.Create();

        public SavingSystem() { }

        public bool Register(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            if (_registeredEntities.Add(entity) == true)
            {
                if (TryLoadEntityState(entity, out string state) == true) entity.SetState(state);

                entity.SavingRequested += (sender, args) => SaveEntityState(entity);

                return true;
            }

            return false;
        }

        public bool Deregister(ISavable entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            if (_registeredEntities.Remove(entity) == true)
            {
                SaveEntityState(entity);
                entity.SavingRequested -= (sender, args) => SaveEntityState(entity);

                return true;
            }

            return false;
        }

        private void SaveEntityState(ISavable entity)
        {
            string state = entity.GetState();

            UTF8Encoding UTF8 = new(true, true);
            byte[] byteState = UTF8.GetBytes(state);

            byte[] encryptedState = Encrypt(byteState);
            string encryptedStateAsBase64 = Convert.ToBase64String(encryptedState);

            string savePath = GetSavePath(entity);
            File.WriteAllText(savePath, encryptedStateAsBase64);
        }

        private bool TryLoadEntityState(ISavable entity, out string state)
        {
            string savePath = GetSavePath(entity);

            if (File.Exists(savePath))
            {
                try
                {
                    string encryptedStateAsBase64 = File.ReadAllText(savePath);
                    byte[] encryptedState = Convert.FromBase64String(encryptedStateAsBase64);
                    byte[] decryptedState = Decrypt(encryptedState);

                    UTF8Encoding UTF8 = new(true, true);

                    state = UTF8.GetString(decryptedState);
                    return true;
                }
                catch (Exception)
                {
                    state = string.Empty;
                    return false;
                }
            }

            state = string.Empty;
            return false;
        }

        private string GetSavePath(ISavable entity) => Path.Combine(SavesDirectory, entity.ID + SavesExtension);

        private byte[] Encrypt(byte[] input)
        {
            byte[] key = new byte[KeyLength];
            _randomKeyGenerator.GetBytes(key);

            List<byte> encryptedInput = new(input.Length + KeyLength);

            for (int i = 0; i < input.Length; i++)
            {
                byte encryptedByte = (byte)(input[i] ^ key[i % KeyLength]);
                encryptedInput.Add(encryptedByte);
            }

            encryptedInput.AddRange(key);

            return encryptedInput.ToArray();
        }

        private byte[] Decrypt(byte[] input)
        {
            if (input.Length < KeyLength) throw new Exception("Passed input is corrupted or invalid!");

            int firstKeyByteIndex = input.Length - KeyLength;
            byte[] key = input[firstKeyByteIndex..];
            byte[] encryptedInput = input[..firstKeyByteIndex];
            byte[] decryptedInput = new byte[encryptedInput.Length];

            for (int i = 0; i < encryptedInput.Length; i++)
            {
                byte decryptedByte = (byte)(input[i] ^ key[i % KeyLength]);
                decryptedInput[i] = decryptedByte;
            }

            return decryptedInput;
        }
    }
}