using YouShallNotPassBackend.Security;

namespace YouShallNotPassBackend.Storage
{
    public record StorageEntry()
    {
        public EntryMetadata Metadata { get; init; } = new EntryMetadata();

        public EncryptedData Data { get; init; } = new EncryptedData();

        public EncryptedData Label { get; init; } = new EncryptedData();

        public EncryptedData? SecurityQuestionAnswer { get; init; } = new EncryptedData();

        public StorageEntry IncrementTimesAccessed()
        {
            return this with { Metadata = Metadata.IncrementTimesAccessed() };
        }
    }
}
