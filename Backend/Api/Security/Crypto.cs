using System.Security.Cryptography;

namespace YouShallNotPassBackend.Security
{
    public class Crypto
    {
        private readonly byte[] serverKey;

        public Crypto(byte[] serverKey)
        {
            this.serverKey = serverKey;
        }

        public byte[] Encrypt(byte[] data, byte[] key, out byte[] iv)
        {
            byte[] encryptionKey = GetEncryptionKey(key);
            byte[] encrypted;

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

            return encrypted;
        }

        public byte[] Decrypt(byte[] encryptedData, byte[] key, byte[] iv, int dataLength)
        {
            byte[] encryptionKey = GetEncryptionKey(key);

            using Aes aesAlg = Aes.Create();

            aesAlg.Key = encryptionKey;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream inputMemoryStream = new(encryptedData);
            using MemoryStream outputMemoryStream = new();
            using CryptoStream cryptoStream = new(inputMemoryStream, decryptor, CryptoStreamMode.Read);

            cryptoStream.CopyTo(outputMemoryStream);

            byte[] data = outputMemoryStream.ToArray();

            if (data.Length != dataLength)
            {
                throw new InvalidOperationException($"bytesRead = {data.Length} != dataLength = {dataLength}");
            }

            return data;
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
