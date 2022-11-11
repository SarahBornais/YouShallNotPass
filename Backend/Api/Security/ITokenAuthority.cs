namespace YouShallNotPassBackend.Security
{
    public interface ITokenAuthority
    {
        public string GetToken(string identity);

        public string? ValidateTokenAndGetIdentity(string token);
    }
}
