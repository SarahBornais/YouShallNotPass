using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    public record EntryMetadata
    {
        public Guid Id { get; init; }

        public ByteString EntryKeyHash { get; init; } = Array.Empty<byte>();

        public ContentType ContentType { get; init; }

        public DateTime ExpirationDate { get; init; }

        public int MaxAccessCount { get; init; }

        public int TimesAccessed { get; init; } = 0;

        public string? SecurityQuestion { get; init; }

        public EntryMetadata IncrementTimesAccessed()
        {
            return this with { TimesAccessed = TimesAccessed + 1 };
        }

        public bool IsEntryExpired()
        {
            return ExpirationDate < DateTime.Now || TimesAccessed >= MaxAccessCount;
        }
    }
}