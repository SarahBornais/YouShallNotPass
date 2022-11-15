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

        public string? SecurityQuestionAnswer { get; init; }

        public byte[] KeyBytes() => Convert.FromHexString(Key);

        public static ContentKey GenerateRandom(string? securityQuestionAnswer)
        {
            return new ContentKey 
            { 
                Id = Guid.NewGuid(), 
                Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(128 / 8)),
                SecurityQuestionAnswer = securityQuestionAnswer
            };
        }

        public Dictionary<string, string> ToQueryParameters()
        {
            Dictionary<string, string> parameters = new()
            {
                ["Id"] = Id.ToString(),
                ["Key"] = Key
            };

            if (SecurityQuestionAnswer != null)
            {
                parameters.Add("SecurityQuestionAnswer", SecurityQuestionAnswer);
            }

            return parameters;
        }
    }
}
