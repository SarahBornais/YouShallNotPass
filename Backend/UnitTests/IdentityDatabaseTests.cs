using Microsoft.Data.Sqlite;
using YouShallNotPassBackend.Security;

namespace YouShallNotPassBackendUnitTests
{
    [TestClass]
    public class IdentityDatabaseTests
    {
        // upper case hex strings
        private readonly Dictionary<string, string> u1 = new()
        {
            ["Key"] = "A1E3A58CB6E3FDB9C82073F4A8E71DA9026BA6C2CA1764FB71A30C3E",
            ["UserName"] = "slack",
            ["Salt"] = "C0C84651",
            ["Hash"] = "EAAF8C960FBAD43B56D10BF564990D2AD67DA78C4AB0A9FC9DCB0395799412DF",
            ["CaptchaVerificationRequired"] = false.ToString()
        };

        // lower case hex strings
        private readonly Dictionary<string, string> u2 = new()
        {
            ["Key"] = "e89942d9ce31d467f91eab8747c0d2c8483fc7e5b329de4375d6a2a9",
            ["UserName"] = "gmail",
            ["Salt"] = "57e8b169",
            ["Hash"] = "0f4292f697070a98f50838ab462b06e9def0fcbe6cf0c4ca0b8f7263d917d63a",
            ["CaptchaVerificationRequired"] = true.ToString()
        };

        private readonly IdentityDatabase identityManager;

        public IdentityDatabaseTests()
        {
            string fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.db");

            identityManager = new(fileLocation);

            using SqliteConnection connection = new($"Data Source={fileLocation}");
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS users (
                username STRING PRIMARY KEY NOT NULL,
                salt STRING NOT NULL,
                hash STRING NOT NULL,
                captchaVerificationRequired BOOLEAN NOT NULL);
            ";

            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
            $@"
                DELETE FROM users;
            ";

            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
            $@"
                INSERT INTO users VALUES (""{u1["UserName"]}"", ""{u1["Salt"]}"", ""{u1["Hash"]}"", {u1["CaptchaVerificationRequired"]});
            ";

            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText =
            $@"
                INSERT INTO users VALUES (""{u2["UserName"]}"", ""{u2["Salt"]}"", ""{u2["Hash"]}"", {u2["CaptchaVerificationRequired"]});
            ";

            command.ExecuteNonQuery();
        }

        [TestMethod]
        public void TestGetSaltAndHash()
        {
            Assert.AreEqual((u1["Salt"], u1["Hash"]), identityManager.GetSaltAndHash(u1["UserName"]));
            Assert.AreEqual((u2["Salt"], u2["Hash"]), identityManager.GetSaltAndHash(u2["UserName"]));
        }

        [TestMethod]
        public void TestAuthSuccess()
        {
            Assert.IsTrue(identityManager.Authenticate(new()
            {
                ServiceName = u1["UserName"],
                SecretKey = u1["Key"]
            }));

            Assert.IsTrue(identityManager.Authenticate(new()
            {
                ServiceName = u2["UserName"],
                SecretKey = u2["Key"]
            }));
        }

        [TestMethod]
        public void TestAuthFailure()
        {
            Assert.IsFalse(identityManager.Authenticate(new()
            {
                ServiceName = u1["UserName"],
                SecretKey = u2["Key"]
            }));

            Assert.IsFalse(identityManager.Authenticate(new()
            {
                ServiceName = u2["UserName"],
                SecretKey = u1["Key"]
            }));
        }

        [TestMethod]
        public void TestUserExists()
        {
            Assert.IsTrue(identityManager.UserExists(u1["UserName"]));
            Assert.IsTrue(identityManager.UserExists(u2["UserName"]));
        }

        [TestMethod]
        public void TestUserRequiresCaptchaVerification()
        {
            Assert.AreEqual(bool.Parse(u1["CaptchaVerificationRequired"]), 
                identityManager.UserRequiresCaptchaVerification(u1["UserName"]));

            Assert.AreEqual(bool.Parse(u2["CaptchaVerificationRequired"]),
                identityManager.UserRequiresCaptchaVerification(u2["UserName"]));
        }
    }
}
