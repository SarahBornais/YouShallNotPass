using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendTests
{
    [TestClass]
    public class StorageManagerTests
    {
        private readonly StorageManager storageManager;

        public StorageManagerTests()
        {
            string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
            Directory.CreateDirectory(entriesLocation);

            Crypto crypto = new(Convert.ToHexString(RandomNumberGenerator.GetBytes(128 / 8)));
            Storage storage = new(entriesLocation);
            storageManager = new(storage, crypto);
        }

        [TestMethod]
        public void TestAddGetEntry()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            CollectionAssert.AreEquivalent(content.Data, retreivedContent.Data);
        }

        [TestMethod]
        public void TestMaxAccessCountOf1()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            CollectionAssert.AreEquivalent(content.Data, retreivedContent.Data);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestMaxAccessCountOf3()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 3);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent.Data);

            Content retreivedContent2 = storageManager.GetEntry(contentKey);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent2.Data);

            Content retreivedContent3 = storageManager.GetEntry(contentKey);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent3.Data);

            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestExpirationDate()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(2), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            for (int i = 0; i < 5; i++)
            {
                Content retreivedContent = storageManager.GetEntry(contentKey);
                CollectionAssert.AreEquivalent(content.Data, retreivedContent.Data);
            }

            Thread.Sleep(2000);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestDelete()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(2), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent.Data);

            bool success = storageManager.DeleteEntry(contentKey.Id);
            Assert.IsTrue(success);
            
            Assert.ThrowsException<EntryNotFoundException>(() => storageManager.GetEntry(contentKey));
        }

        private static Content GetContent(DateTime expirationDate, int maxAccessCount)
        {
            byte[] data = Encoding.ASCII.GetBytes("passowrd");
            string label = "my passowrd";

            return new()
            {
                ContentType = ContentType.TEXT,
                Label = label,
                ExpirationDate = expirationDate,
                MaxAccessCount = maxAccessCount,
                Data = data
            };
        }
    }
}
