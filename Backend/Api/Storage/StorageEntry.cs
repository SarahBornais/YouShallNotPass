using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    [Serializable()]
    public class StorageEntry
    {
        public Guid Id { get; set; }

        public ContentType ContentType { get; set; }

        public byte[] EntryKeyHash { get; set; }

        public FileEntry EncryptedFileEntry { get; set; }

        public byte[] LabelIV { get; set; }

        public byte[] DataIV { get; set; }

        public int LabelLength { get; set; }

        public int DataLength { get; set; }
    }
}
