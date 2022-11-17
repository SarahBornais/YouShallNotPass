using System.ComponentModel.DataAnnotations;

namespace YouShallNotPassBackend.DataContracts
{
    public class Service
    {
        [Required]
        public string ServiceName { get; init; } = string.Empty;

        [Required]
        public string SecretKey { get; init; } = string.Empty;
    }
}
