using System.Net;
using System.Text;
using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class VaultTests : ApiTest
    {
        private const string path = "vault";
        private static readonly Random random = new();

        public VaultTests() : base(requireAuthentication: true)
        {
        }

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
            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public async Task TestPostGetWithLongerLabel()
        {
            Content content = GetContent("password", "Password from Slack", DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public async Task TestPostGetWithEvenLongerLabel()
        {
            string label = RandomString(1024);
            Content content = GetContent("password", label, DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);
        }

        [TestMethod]
        public async Task TestMaxAccessCountOf1()
        {
            Content content = GetContent(DateTime.Now.AddMinutes(15), 1);

            ContentKey? contentKey = await PostSuccess<Content, ContentKey>(path, content);
            ValidateContentKey(contentKey);

            Content? retreivedContent = await GetSuccess<Content>(path, contentKey!.ToQueryParameters());
            ValidateContent(retreivedContent);
            Assert.AreEqual(content, retreivedContent);

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
                Assert.AreEqual(content, retreivedContent);
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
                Assert.AreEqual(content, retreivedContent);
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
            Assert.AreEqual(content, retreivedContent);

            await DeleteSuccess(path, contentKey!.ToQueryParameters());
            await GetFailure(path, contentKey!.ToQueryParameters(), HttpStatusCode.NotFound);
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

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
