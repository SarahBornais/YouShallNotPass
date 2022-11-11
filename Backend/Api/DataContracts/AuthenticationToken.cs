namespace YouShallNotPassBackend.DataContracts
{
    public record AuthenticationToken
    {
        public string Token { get; init; } = string.Empty;

        public DateTime ExpirationDate { get; init; }
    }
}
