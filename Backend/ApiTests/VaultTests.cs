using System.Net;
using System.Text;
using System.Text.Json;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class VaultTests : ApiTest
    {
        private const string path = "vault";

        [TestMethod]
        public async Task TestPost()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);
            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);

            ValidateContentKey(contentKey);
        }

        [TestMethod]
        public async Task TestPostGet()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent!.Data);
        }

        [TestMethod]
        public async Task TestMaxAccessCountOf1()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent!.Data);

            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestMaxAccessCountOf3()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 3);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            for (int i = 0; i < 3; i++)
            {
                Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
                ValidateContent(retreivedContent);
                CollectionAssert.AreEquivalent(content.Data, retreivedContent!.Data);
            }

            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestExpirationDate()
        {
            Content content = GetContent(DateTime.Now.AddSeconds(5), 10);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            for (int i = 0; i < 5; i++)
            {
                Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
                ValidateContent(retreivedContent);
                CollectionAssert.AreEquivalent(content.Data, retreivedContent!.Data);
            }

            Thread.Sleep(5000);
            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task TestPostGetDelete()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            CollectionAssert.AreEquivalent(content.Data, retreivedContent!.Data);

            await DeleteSuccess(path, contentKey!.ToQueryParameters());
            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
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
