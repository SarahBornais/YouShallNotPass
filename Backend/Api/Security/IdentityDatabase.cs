using Microsoft.Data.Sqlite;
using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Security
{
    public class IdentityDatabase : IAuthenticator
    {
        private readonly string dbFile;

        public IdentityDatabase(string dbFile)
        {
            this.dbFile = dbFile;
        }

        public bool Authenticate(Service service)
        {
            (string salt, string expectedHash) = GetSaltAndHash(service.ServiceName);
            byte[] actualHash = Crypto.Hash(Convert.FromHexString(salt + service.SecretKey));

            return Enumerable.SequenceEqual(Convert.FromHexString(expectedHash), actualHash);
        }

        public bool UserExists(string username)
        {
            using var connection = new SqliteConnection($"Data Source={dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT EXISTS(SELECT 1 FROM users WHERE username=$username);
            ";
            command.Parameters.AddWithValue("$username", username);

            return (long) (command.ExecuteScalar() ?? 0L) == 1;
        }

        public bool UserRequiresCaptchaVerification(string username)
        {
            using var connection = new SqliteConnection($"Data Source={dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT captchaVerificationRequired
                FROM users
                WHERE username = $username
            ";
            command.Parameters.AddWithValue("$username", username);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetBoolean(0);
            }

            throw new ArgumentException($"{username} not found");
        }

        public (string salt, string hash) GetSaltAndHash(string username)
        {
            using var connection = new SqliteConnection($"Data Source={dbFile}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT salt, hash
                FROM users
                WHERE username = $username
            ";
            command.Parameters.AddWithValue("$username", username);

            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                return (reader.GetString(0), reader.GetString(1));
            }

            throw new ArgumentException($"{username} not found");
        }
    }
}
