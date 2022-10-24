using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendTests
{
    [TestClass]
    public class CryptoTests
    {
        private readonly Crypto crypto = new(Convert.ToHexString(RandomNumberGenerator.GetBytes(128 / 8)));
        private readonly byte[] key = RandomNumberGenerator.GetBytes(128 / 8);

        [TestMethod]
        public void TestEncryptDecrypt1Byte()
        {
            byte[] data = RandomNumberGenerator.GetBytes(20);
            byte[] decrypted = EncryptDecrypt(data);

            CollectionAssert.AreEquivalent(data, decrypted);
        }

        [TestMethod]
        public void TestEncryptDecrypt2Bytes()
        {
            byte[] data = RandomNumberGenerator.GetBytes(2);
            byte[] decrypted = EncryptDecrypt(data);

            CollectionAssert.AreEquivalent(data, decrypted);
        }

        [TestMethod]
        public void TestEncryptDecrypt8Bytes()
        {
            byte[] data = RandomNumberGenerator.GetBytes(8);
            byte[] decrypted = EncryptDecrypt(data);

            CollectionAssert.AreEquivalent(data, decrypted);
        }

        [TestMethod]
        public void TestEncryptDecrypt20bytes()
        {
            byte[] data = RandomNumberGenerator.GetBytes(20);
            byte[] decrypted = EncryptDecrypt(data);

            CollectionAssert.AreEquivalent(data, decrypted);
        }

        [TestMethod]
        public void TestEncryptDecrypt1024bytes()
        {
            byte[] data = RandomNumberGenerator.GetBytes(1024);
            byte[] decrypted = EncryptDecrypt(data);

            CollectionAssert.AreEquivalent(data, decrypted);
        }

        private byte[] EncryptDecrypt(byte[] data)
        {
            byte[] encrypted = crypto.Encrypt(data, key, out byte[] iv);
            return crypto.Decrypt(encrypted, key, iv, data.Length);
        }
    }
}