using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Cryptography;
using System.Text;

namespace YouShallNotPassBackend.Security
{
    public class Crypto
    {
        private readonly byte[] serverKey;
        private readonly Encoding encoding = Encoding.Unicode;

        public Crypto(byte[] serverKey)
        {
            this.serverKey = serverKey;
        }

        public EncryptedData Encrypt(byte[] data, byte[] key)
        {
            byte[] encryptionKey = GetEncryptionKey(key);
            byte[] encrypted, iv;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = encryptionKey;
                iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using MemoryStream memoryStream = new(data.Length);
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                encrypted = memoryStream.ToArray();
            }

            return new EncryptedData
            {
                Data = encrypted,
                IV = iv,
                Length = data.Length
            };
        }

        public byte[] Decrypt(EncryptedData encryptedData, byte[] key)
        {
            byte[] encryptionKey = GetEncryptionKey(key);

            using Aes aesAlg = Aes.Create();

            aesAlg.Key = encryptionKey;
            aesAlg.IV = encryptedData.IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream inputMemoryStream = new(encryptedData.Data);
            using MemoryStream outputMemoryStream = new();
            using CryptoStream cryptoStream = new(inputMemoryStream, decryptor, CryptoStreamMode.Read);

            cryptoStream.CopyTo(outputMemoryStream);

            byte[] data = outputMemoryStream.ToArray();

            if (data.Length != encryptedData.Length)
            {
                throw new InvalidOperationException($"bytesRead = {data.Length} != dataLength = {encryptedData.Length}");
            }

            return data;
        }

        public EncryptedData EncryptString(string data, byte[] key)
        {
            return Encrypt(encoding.GetBytes(data), key);
        }

        public string DecryptToString(EncryptedData encryptedData, byte[] key)
        {
            return encoding.GetString(Decrypt(encryptedData, key));
        }

        public static byte[] Hash(byte[] bytes)
        {
            return SHA256.HashData(bytes);
        }

        private byte[] GetEncryptionKey(byte[] contentKey)
        {
            if (contentKey.Length != 128 / 8 || serverKey.Length != 128 / 8)
            {
                throw new InvalidOperationException($"content key length = {contentKey.Length}, server key length = {serverKey.Length}");
            }

            byte[] encryptionKey = new byte[256 / 8];

            Buffer.BlockCopy(contentKey, 0, encryptionKey, 0, 128 / 8);
            Buffer.BlockCopy(serverKey, 0, encryptionKey, 128 / 8, 128 / 8);

            return encryptionKey;
        }
    }
}
