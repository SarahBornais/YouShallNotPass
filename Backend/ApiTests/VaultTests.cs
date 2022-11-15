using System.Net;
using System.Text;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;
using YouShallNotPassBackendUnitTests;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class VaultTests : ApiTest
    {
        private const string path = "vault";

        public VaultTests() : base(requireAuthentication: true)
        {
        }

        [TestMethod]
        public async Task TestPost()
        {
            Content content = StorageManagerTests.GetContent();
            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);

            ValidateContentKey(contentKey);
        }

        [TestMethod]
        public async Task TestPostGet()
        {
            Content content = StorageManagerTests.GetContent();

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public async Task TestMaxAccessCountOf1()
        {
            Content content = StorageManagerTests.GetContent(maxAccessCount: 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);

            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestMaxAccessCountOf3()
        {
            Content content = StorageManagerTests.GetContent(maxAccessCount: 3);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            for (int i = 0; i < 3; i++)
            {
                Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
                ValidateContent(retreivedContent);
                Assert.AreEqual(content, retreivedContent);
            }

            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestExpirationDate()
        {
            Content content = StorageManagerTests.GetContent(expirationDate: DateTime.Now.AddSeconds(5));

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);
            
            Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);

            Thread.Sleep(5000);
            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestPostGetDelete()
        {
            Content content = StorageManagerTests.GetContent();

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);

            await DeleteSuccess(path, contentKey!.ToQueryParameters());
            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestSecurityQuestion()
        {
            Content content = StorageManagerTests.GetContent(securityQuestionAnswer: "red");
            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            await GetFailure(path,
                new ContentKey()
                {
                    Id = contentKey!.Id,
                    Key = contentKey.Key,
                    SecurityQuestionAnswer = "green"
                }.ToQueryParameters(),
                HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TestGetSecurityQuestion()
        {
            string question = "fav food?";
            string answer = "pizza";
            Content content = StorageManagerTests.GetContent(securityQuestion: question, securityQuestionAnswer: answer);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);
            Assert.AreEqual(answer, contentKey!.SecurityQuestionAnswer);

            string? retreivedQuestion = await GetSuccessAsString(path + "/securityQuestion", new()
            {
                ["Id"] = contentKey!.Id.ToString()
            });

            Assert.IsNotNull(retreivedQuestion);
            Assert.AreEqual(question, retreivedQuestion);

            Content? retreivedContent = await GetSuccessAsJson<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);
        }

        private void ValidateContentKey(ContentKey? contentKey)
        {
            Assert.IsNotNull(contentKey);
            Assert.AreEqual(128 / 8, contentKey.KeyBytes().Length);
        }

        private void ValidateContent(Content? content)
        {
            Assert.IsNotNull(content);
            Assert.IsNotNull(content.Label);
            Assert.IsNotNull(content.ExpirationDate);
            Assert.IsNotNull(content.Data);
        }
    }
}
