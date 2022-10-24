using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouShallNotPassBackendApiTests
{
    [TestClass]
    public class IsAliveTests : ApiTest
    {
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
