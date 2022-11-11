using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class SecurityTests : ApiTest
    {
        public SecurityTests() : base(requireAuthentication: false)
        {
        }

        [TestMethod]
        public async Task TestAuthenticate()
        {
            await GetToken();
        }
    }
}
