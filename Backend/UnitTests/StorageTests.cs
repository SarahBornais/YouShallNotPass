using Aornis;
using System.Security.Cryptography;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendUnitTests
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

        [TestCleanup()]
        public void Cleanup()
        {
            storage.Clear();
        }

        [TestMethod]
        public void TestReadWrite()
        {
            StorageEntry storageEntry = GetStorageEntry();

            storage.Write(storageEntry);
            Optional<StorageEntry> retreivedStorageEntry = storage.Read(storageEntry.Metadata.Id);
            Assert.IsTrue(retreivedStorageEntry.HasValue);
            Assert.AreEqual(storageEntry, retreivedStorageEntry.Value);
        }

        [TestMethod]
        public void TestContains()
        {
            StorageEntry storageEntry = GetStorageEntry();

            storage.Write(storageEntry);
            Assert.IsTrue(storage.Contains(storageEntry.Metadata.Id));
        }

        [TestMethod]
        public void TestCount()
        {
            Assert.AreEqual(0, storage.Count());
            
            StorageEntry storageEntry = GetStorageEntry();
            storage.Write(storageEntry);

            Assert.AreEqual(1, storage.Count());

            for (int i = 0; i < 5; i++)
            {
                StorageEntry otherStorageEntry = GetStorageEntry();
                storage.Write(otherStorageEntry);
            }

            Assert.AreEqual(6, storage.Count());
        }

        [TestMethod]
        public void TestGetAllEntryGuids()
        {
            List<Guid> entryGuids = new();

            for (int i = 0; i < 5; i++)
            {
                StorageEntry storageEntry = GetStorageEntry();
                storage.Write(storageEntry);
                entryGuids.Add(storageEntry.Metadata.Id);
            }

            CollectionAssert.AreEquivalent(entryGuids, storage.GetAllEntryGuids());
        }

        [TestMethod]
        public void TestClear()
        {
            storage.Clear();
            Assert.AreEqual(0, storage.Count());
        }

        private StorageEntry GetStorageEntry()
        {
            return new()
            {
                Metadata = new()
                {
                    Id = Guid.NewGuid(),
                    EntryKeyHash = RandomNumberGenerator.GetBytes(256 / 8),
                    ContentType = ContentType.TEXT,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                    MaxAccessCount = 100
                },
                Data = new()
                {
                    Data = RandomNumberGenerator.GetBytes(24),
                    IV = RandomNumberGenerator.GetBytes(12),
                    Length = 10
                },
                Label = new()
                {
                    Data = RandomNumberGenerator.GetBytes(24),
                    IV = RandomNumberGenerator.GetBytes(12),
                    Length = 10
                }
            };
        }
    }
}
