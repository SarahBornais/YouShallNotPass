using System.Security.Cryptography;
using System.Text;
using YouShallNotPassBackend.Security;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackendUnitTests
{
    [TestClass]
    public class StorageManagerTests
    {
        private readonly StorageManager storageManager;
        private readonly StorageManager storageManagerWithGC;
        private static readonly Random random = new();

        public StorageManagerTests()
        {
            string entriesLocation = Path.Combine(Path.GetTempPath(), "entries");
            string entriesLocationWithGC = Path.Combine(Path.GetTempPath(), "entriesWithGC");

            Directory.CreateDirectory(entriesLocation);
            Directory.CreateDirectory(entriesLocationWithGC);

            Crypto crypto = new(Convert.ToHexString(RandomNumberGenerator.GetBytes(128 / 8)));

            Storage storage = new(entriesLocation);
            Storage storageWithGc = new(entriesLocationWithGC);

            storageManager = new(storage, crypto, clearingInvertalMillis: null);
            storageManagerWithGC = new(storageWithGc, crypto, clearingInvertalMillis: 10);   
        }

        [TestCleanup()]
        public void Cleanup()
        {
            storageManager.Clear();
        }

        [TestMethod]
        public void TestAddGetEntry()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestAddGetEntryWithLongerLabel()
        {
            Content content = GetContent("password", "Password from Slack", DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestAddGetEntryWithEvenLongerLabel()
        {
            string label = RandomString(1024);
            Content content = GetContent("password", label, DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestMaxAccessCountOf1()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestMaxAccessCountOf3()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 3);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Content retreivedContent2 = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent2);

            Content retreivedContent3 = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent3);

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
                Assert.AreEqual(content, retreivedContent);
            }

            Thread.Sleep(2001);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
            
        }

        [TestMethod]
        public void TestDelete()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(2), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            bool success = storageManager.DeleteEntry(contentKey.Id);
            Assert.IsTrue(success);
            
            Assert.ThrowsException<EntryNotFoundException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestDeleteExpired()
        {
            StorageManager storageManager = storageManagerWithGC;

            Content content = GetContent(DateTime.Now.AddSeconds(1), 100);
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Thread.Sleep(3000);

            Assert.ThrowsException<EntryNotFoundException>(() => storageManager.GetEntry(contentKey));
        }

        private static Content GetContent(string data, string label, DateTime expirationDate, int maxAccessCount)
        {
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            return new()
            {
                ContentType = ContentType.TEXT,
                Label = label,
                ExpirationDate = expirationDate,
                MaxAccessCount = maxAccessCount,
                Data = dataBytes
            };
        }

        private static Content GetContent(DateTime expirationDate, int maxAccessCount)
        {
            return GetContent("password", "my password", expirationDate, maxAccessCount);
        }
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
