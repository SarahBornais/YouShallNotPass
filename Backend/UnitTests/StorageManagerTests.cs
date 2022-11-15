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

        public StorageManagerTests()
        {
            string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
            string entriesLocationWithGC = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entriesWithGC");

            Directory.CreateDirectory(entriesLocation);
            Directory.CreateDirectory(entriesLocationWithGC);

            Crypto crypto = new(RandomNumberGenerator.GetBytes(128 / 8));

            Storage storage = new(entriesLocation);
            Storage storageWithGC = new(entriesLocationWithGC);

            storageManager = new(storage, crypto, clearingInvertalMillis: null);
            storageManagerWithGC = new(storageWithGC, crypto, clearingInvertalMillis: 1);   
        }

        [TestCleanup()]
        public void Cleanup()
        {
            storageManager.Clear();
        }

        [TestMethod]
        public void TestAddGetEntry()
        {
            Content content = GetContent();
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestAddGetEntryWithLongerLabel()
        {
            Content content = GetContent(label: "Password from Slack");

            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public void TestMaxAccessCountOf1()
        {
            Content content = GetContent(maxAccessCount: 1);
            ContentKey contentKey = storageManager.AddEntry(content);
            Content retreivedContent = storageManager.GetEntry(contentKey);

            Assert.AreEqual(content, retreivedContent);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestMaxAccessCountOf3()
        {
            Content content = GetContent(maxAccessCount: 3);
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
            Content content = GetContent(expirationDate: DateTime.Now.AddSeconds(2));
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Thread.Sleep(2001);
            Assert.ThrowsException<EntryExpiredException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestDelete()
        {
            Content content = GetContent();
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

            Content content = GetContent(expirationDate: DateTime.Now.AddSeconds(1));
            ContentKey contentKey = storageManager.AddEntry(content);

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);

            Thread.Sleep(3000);

            Assert.ThrowsException<EntryNotFoundException>(() => storageManager.GetEntry(contentKey));
        }

        [TestMethod]
        public void TestSecurityQuestion()
        {
            Content content = GetContent(securityQuestionAnswer: "red");
            ContentKey contentKey = storageManager.AddEntry(content);

            Assert.ThrowsException<InvalidSecurityQuestionAnswerException>(() =>
                storageManager.GetEntry(new ContentKey()
                {
                    Id = contentKey.Id,
                    Key = contentKey.Key,
                    SecurityQuestionAnswer = "green"
                }));
        }

        [TestMethod]
        public void TestGetSecurityQuestion()
        {
            string question = "fav food?";
            string answer = "pizza";
            Content content = GetContent(securityQuestion: question, securityQuestionAnswer: answer);

            ContentKey contentKey = storageManager.AddEntry(content);

            Assert.AreEqual(answer, contentKey.SecurityQuestionAnswer);
            Assert.AreEqual(question, storageManager.GetSecurityQuestion(contentKey.Id));

            Content retreivedContent = storageManager.GetEntry(contentKey);
            Assert.AreEqual(content, retreivedContent);
        }

        public static Content GetContent(
            DateTime? expirationDate = null, 
            string data = "password", 
            string label = "my password", 
            int maxAccessCount = 20, 
            string? securityQuestion = null, 
            string? securityQuestionAnswer = null)
        {
            return new()
            {
                ContentType = ContentType.TEXT,
                Label = label,
                ExpirationDate = expirationDate ?? DateTime.Now.AddMinutes(5),
                MaxAccessCount = maxAccessCount,
                Data = Encoding.ASCII.GetBytes(data),
                SecurityQuestion = securityQuestion,
                SecurityQuestionAnswer = securityQuestionAnswer
            };
        }
    }
}
