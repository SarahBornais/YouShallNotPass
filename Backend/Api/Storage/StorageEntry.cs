namespace YouShallNotPassBackend.Storage
{
    [Serializable()]
    public class StorageEntry
    {
        public Guid Id { get; set; }

        public EntryMetadata EntryMetadata { get; set; }

        public byte[] EntryKeyHash { get; set; }

        public FileEntry EncryptedFileEntry { get; set; }

        public byte[] LabelIV { get; set; }

        public byte[] DataIV { get; set; }

        public int LabelLength { get; set; }

        public int DataLength { get; set; }

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
