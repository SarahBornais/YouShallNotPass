namespace YouShallNotPassBackend.Storage
{
    public class StorageEntry
    {
        public Guid Id { get; init; }

        public EntryMetadata EntryMetadata { get; init; } = new EntryMetadata();

        public byte[] EntryKeyHash { get; init; } = Array.Empty<byte>();

        public FileEntry EncryptedFileEntry { get; init; } = new FileEntry();

        public byte[] LabelIV { get; init; } = Array.Empty<byte>();

        public byte[] DataIV { get; init; } = Array.Empty<byte>();

        public int LabelLength { get; init; }

        public int DataLength { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is not StorageEntry other) return false;

            return Id.Equals(other.Id) &&
                EntryMetadata.Equals(other.EntryMetadata) &&
                Enumerable.SequenceEqual(EntryKeyHash, other.EntryKeyHash) &&
                EncryptedFileEntry.Equals(other.EncryptedFileEntry) &&
                Enumerable.SequenceEqual(LabelIV, other.LabelIV) &&
                Enumerable.SequenceEqual(DataIV, other.DataIV) &&
                LabelLength == other.LabelLength &&
                DataLength == other.DataLength;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() +
                EntryMetadata.GetHashCode() +
                EntryKeyHash.GetHashCode() +
                LabelIV.GetHashCode() +
                DataIV.GetHashCode() +
                LabelLength.GetHashCode() +
                DataLength.GetHashCode();
        }
    }
}
