using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouShallNotPassBackend.Security
{
    public class AuthenticationMiddleware
    {
        private record CaptchaResponse()
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("challenge_ts")]
            public DateTime ChallengeTimestamp { get; set; }

            [JsonPropertyName("hostname")]
            public string Hostname { get; set; } = string.Empty;

            [JsonPropertyName("error-codes")]
            public List<string> ErrorCodes { get; set; } = new List<string>();
        }

        private const string schemePrefix = "Bearer ";

        private const string captchaTokenHeader = "CaptchaToken";
        private const string captchaVerificationBaseUrl = "https://www.google.com";
        private const string captchaVerificationUrlPath = "recaptcha/api/siteverify";

        private readonly RequestDelegate next;
        private readonly ITokenAuthority tokenAuthority;

        private readonly string captchaApiKey;
        private readonly HttpClient httpClient;    

        public AuthenticationMiddleware(RequestDelegate next, ITokenAuthority tokenAuthority, IConfiguration configuration)
        {
            this.next = next;
            this.tokenAuthority = tokenAuthority;

            captchaApiKey = configuration["CaptchaApiKey"] ?? throw new ArgumentException("CaptchaApiKey not defined");
            httpClient = new HttpClient();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? captchaToken = GetCaptchaTokenFromHeader(context);
            if (captchaToken != null)
            {
                if (await ValidateCaptchaToken(captchaToken))
                {
                    context.Items["User"] = "An actual person";
                }
            }
            else
            {
                string? bearerToken = GetBearerTokenFromHeader(context);
                if (bearerToken != null) context.Items["User"] = tokenAuthority.ValidateTokenAndGetIdentity(bearerToken);
            }

            await next(context);
        }

        private static string? GetBearerTokenFromHeader(HttpContext context)
        {
            string headerValue = context.Request.Headers["Authorization"];
            if (headerValue == null || !headerValue.StartsWith(schemePrefix)) return null;

            return headerValue[schemePrefix.Length..];
        }

        private static string? GetCaptchaTokenFromHeader(HttpContext context)
        {
            return context.Request.Headers[captchaTokenHeader];
        }

        public async Task<bool> ValidateCaptchaToken(string token)
        {
            Uri uri = CalculateUri(captchaVerificationBaseUrl, captchaVerificationUrlPath, new()
            {
                ["secret"] = captchaApiKey,
                ["response"] = token
            });

            HttpResponseMessage response = await httpClient.PostAsync(uri, new StringContent(string.Empty));
            if (response.StatusCode != HttpStatusCode.OK) return false;

            string responseJson = await response.Content.ReadAsStringAsync();
            CaptchaResponse? responseObject = JsonSerializer.Deserialize<CaptchaResponse>(responseJson);

            return responseObject?.Success ?? false;
        }

        public static Uri CalculateUri(string apiUrl, string path, Dictionary<string, string>? queryParameters)
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
