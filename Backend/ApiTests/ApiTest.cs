using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        [TestInitialize]
        public void SetupTest()
        {
            apiUrl = (string)TestContext.Properties["apiUrl"];
            client.BaseAddress = new Uri(apiUrl);
        }
#nullable enable warnings

        public async Task<TResponseBody?> GetSuccess<TResponseBody>(string path, Dictionary<string, string> queryParameters)
        {
            HttpResponseMessage response = await client.GetAsync(CalculateUri(path, queryParameters));
            response.EnsureSuccessStatusCode();

            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponseBody>(responseJson, jsonOptions);
        }

        public async Task GetFailure(string path, Dictionary<string, string> queryParameters, HttpStatusCode expectedStatusCode)
        {
            HttpResponseMessage response = await client.GetAsync(CalculateUri(path, queryParameters));
            Assert.AreEqual(response.StatusCode, expectedStatusCode);
        }

        public async Task GetSuccess(string path, Dictionary<string, string>? queryParameters)
        {
            HttpResponseMessage response = await client.GetAsync(CalculateUri(path, queryParameters));
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
            HttpResponseMessage response = await client.DeleteAsync(CalculateUri(path, queryParameters));
            response.EnsureSuccessStatusCode();
        }

        private Uri CalculateUri(string path, Dictionary<string, string>? queryParameters)
        {
            UriBuilder builder = new(apiUrl)
            {
                Path = path
            };

            if (queryParameters == null)
            {
                return builder.Uri;
            }

            foreach (KeyValuePair<string, string> parameter in queryParameters)
            {
                if (builder.Query != null && builder.Query.Length > 1)
                {
                    builder.Query = builder.Query + "&" + $"{parameter.Key}={parameter.Value}";
                }
                else
                {
                    builder.Query = $"{parameter.Key}={parameter.Value}";
                }
            }

            return builder.Uri;
        }
    }
}