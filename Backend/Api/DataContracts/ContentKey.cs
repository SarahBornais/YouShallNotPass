using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace YouShallNotPassBackend.DataContracts
{
    [DataContract]
    public class ContentKey
    {
        [DataMember(IsRequired = true)]
        public Guid Id { get; set; }

        [DataMember(IsRequired = true)]
        public string Key { get; set; }

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
