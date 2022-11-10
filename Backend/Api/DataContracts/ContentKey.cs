using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace YouShallNotPassBackend.DataContracts
{
    public class ContentKey
    {
        /// <summary>
        ///  UUID-formatted string
        /// </summary>
        [Required]
        public Guid Id { get; init; }

        /// <summary>
        ///  Hex-fromatted string
        /// </summary>
        [Required]
        public string Key { get; init; } = string.Empty;

        public byte[] KeyBytes() => Convert.FromHexString(Key);

        public static ContentKey GenerateRandom()
        {
            byte[] random = RandomNumberGenerator.GetBytes(128 / 8);
            return new ContentKey { Id = Guid.NewGuid(), Key = Convert.ToHexString(random) };
        }

        public Dictionary<string, string> ToQueryParameters()
        {
            return new Dictionary<string, string>()
            {
                { "Id", Id.ToString() },
                { "Key", Key }
            };
        }
    }
}
