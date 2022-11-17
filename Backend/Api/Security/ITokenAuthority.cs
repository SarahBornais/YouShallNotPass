using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Security
{
    public interface ITokenAuthority
    {
        public AuthenticationToken GetToken(string identity);

        public string? ValidateTokenAndGetIdentity(string token);
    }
}
