namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class IsAliveTests : ApiTest
    {
        public IsAliveTests() : base(requireAuthentication: false)
        {
        }

        [TestMethod]
        public void ApiUrlFromRunSettings()
        {
            Assert.IsNotNull(apiUrl);
        }

        [TestMethod]
        public async Task TestIsAlive()
        {
            await GetSuccess("isAlive", null);
        }
    }
}
