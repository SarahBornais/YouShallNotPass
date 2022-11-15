using Microsoft.Extensions.Configuration;
using YouShallNotPassBackend.Security;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class CaptchaVerificationTest
    {
        [TestMethod]
        public async Task TestVerifyCaptcha()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddUserSecrets<CaptchaVerificationTest>()
                .Build();

            JwtAuthority jwtAuthority = new(string.Empty, string.Empty, Convert.FromHexString("AB"));
            AuthenticationMiddleware middleware = new((_) => Task.CompletedTask, jwtAuthority, configuration);

            Assert.IsFalse(await middleware.ValidateCaptchaToken("junk"));
        }
    }
}
