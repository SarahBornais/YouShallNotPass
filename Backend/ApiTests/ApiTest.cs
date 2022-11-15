using YouShallNotPassBackend.DataContracts;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

using UriUtil = YouShallNotPassBackend.Security.AuthenticationMiddleware;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class ApiTest
    {
#nullable disable warnings // the test framwork will set TestContext
        public TestContext TestContext { get; set; }

        public string apiUrl;
        public readonly HttpClient client = new();
        public readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected readonly string secretKey;

        private AuthenticationToken token;
        private readonly bool requireAuthentication;

        public ApiTest(bool requireAuthentication)
        {
            this.requireAuthentication = requireAuthentication;

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddUserSecrets<ApiTest>()
                .Build();

            secretKey = configuration["ApiKey"];
            Assert.IsNotNull(secretKey);
        }

        [TestInitialize]
        public async Task SetupTest()
        {
            apiUrl = (string)TestContext.Properties["apiUrl"];
            client.BaseAddress = new Uri(apiUrl);

            if (requireAuthentication)
            {
                await RefreshAuthToken();
            }
        }
#nullable enable warnings

        public async Task<TResponseBody?> GetSuccess<TResponseBody>(string path, Dictionary<string, string> queryParameters)
        {
            HttpResponseMessage response = await client.GetAsync(UriUtil.CalculateUri(apiUrl, path, queryParameters));
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponseBody>(responseJson, jsonOptions);
        }

        public async Task GetFailure(string path, Dictionary<string, string> queryParameters, HttpStatusCode expectedStatusCode)
        {
            HttpResponseMessage response = await client.GetAsync(UriUtil.CalculateUri(apiUrl, path, queryParameters));
            Assert.AreEqual(response.StatusCode, expectedStatusCode);
        }

        public async Task GetSuccess(string path, Dictionary<string, string>? queryParameters)
        {
            HttpResponseMessage response = await client.GetAsync(UriUtil.CalculateUri(apiUrl, path, queryParameters));
            response.EnsureSuccessStatusCode();

            return;
        }

        public async Task<TResponseBody?> PostSuccess<TRequestBody, TResponseBody>(string path, TRequestBody body)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(path, body);
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponseBody>(responseJson, jsonOptions);
        }

        public async Task DeleteSuccess(string path, Dictionary<string, string> queryParameters)
        {
            HttpResponseMessage response = await client.DeleteAsync(UriUtil.CalculateUri(apiUrl, path, queryParameters));
            response.EnsureSuccessStatusCode();
        }

        public async Task<AuthenticationToken> GetToken()
        {
            AuthenticationToken? authenticationToken = await GetSuccess<AuthenticationToken>("security/authenticate", new()
            {
                ["ServiceName"] = "test",
                ["SecretKey"] = secretKey
            });

            Assert.IsNotNull(authenticationToken);
            Assert.IsNotNull(authenticationToken.Token);
            Assert.IsTrue(authenticationToken.ExpirationDate - DateTime.UtcNow > TimeSpan.FromMinutes(1));
            Assert.IsTrue(authenticationToken.ExpirationDate - DateTime.UtcNow < TimeSpan.FromHours(1));

            return authenticationToken;
        }

        private async Task RefreshAuthToken()
        {
            if (token == null || token.ExpirationDate < DateTime.UtcNow + TimeSpan.FromMinutes(1))
            {
                token = await GetToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            }
        }
    }
}