using Aornis;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Cryptography;
using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendTests
{
    [TestClass]
    public class StorageTests
    {
        private readonly Storage storage;
        

        public StorageTests()
        {
            string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
            Directory.CreateDirectory(entriesLocation);
            storage = new(entriesLocation);
        }

        [TestMethod]
        public void TestReadWrite()
        {
            Guid id = Guid.NewGuid();
            ContentType contentType = ContentType.TEXT;
            byte[] hash = RandomNumberGenerator.GetBytes(256 / 8);

            FileEntry encryptedFileEntry = new()
            {
                LabelBytes = RandomNumberGenerator.GetBytes(12),
                Data = RandomNumberGenerator.GetBytes(24)
            };

            byte[] labelIV = RandomNumberGenerator.GetBytes(128 / 8);
            byte[] dataIV = RandomNumberGenerator.GetBytes(128 / 8);

            int labelLength = 10;
            int dataLength = 20;

            StorageEntry storageEntry = new()
            {
                Id = id,
                ContentType = contentType,
                EntryKeyHash = hash,
                EncryptedFileEntry = encryptedFileEntry,
                LabelIV = labelIV,
                DataIV = dataIV,
                LabelLength = labelLength,
                DataLength = dataLength,
            };

            storage.Write(storageEntry);
            Optional<StorageEntry> optionalRetreivedStorageEntry = storage.Read(id);

            Assert.IsTrue(optionalRetreivedStorageEntry.HasValue);

            StorageEntry retreivedStorageEntry = optionalRetreivedStorageEntry.Value;

            Assert.AreEqual(contentType, retreivedStorageEntry.ContentType);
            Assert.AreEqual(id, retreivedStorageEntry.Id);
            CollectionAssert.AreEquivalent(hash, retreivedStorageEntry.EntryKeyHash);

            CollectionAssert.AreEquivalent(encryptedFileEntry.LabelBytes, retreivedStorageEntry.EncryptedFileEntry.LabelBytes);
            CollectionAssert.AreEquivalent(encryptedFileEntry.Data, retreivedStorageEntry.EncryptedFileEntry.Data);

            CollectionAssert.AreEquivalent(labelIV, retreivedStorageEntry.LabelIV);
            CollectionAssert.AreEquivalent(dataIV, retreivedStorageEntry.DataIV);

            Assert.AreEqual(labelLength, retreivedStorageEntry.LabelLength);
            Assert.AreEqual(dataLength, retreivedStorageEntry.DataLength);
        }
    }
}
