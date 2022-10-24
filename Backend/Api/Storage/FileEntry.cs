using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Text;
using System.Text.Json.Serialization;
using YouShallNotPassBackend.Cryptography;
using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    [Serializable()]
    public class FileEntry
    {
        public FileEntry(string label, byte[] data)
        {
            LabelBytes = Encoding.Unicode.GetBytes(label);
            Data = data;
        }

        public FileEntry()
        {
            LabelBytes = Array.Empty<byte>();
            Data = Array.Empty<byte>();
        }

        [JsonIgnore]
        public string Label => Encoding.Unicode.GetString(LabelBytes);

        public byte[] LabelBytes { get; set; }

        public byte[] Data { get; set; }

        public static StorageEntry EncryptFileEntry(Crypto crypto, FileEntry fileEntry, byte[] key, ContentType contentType, Guid id)
        {
            return new StorageEntry()
            {
                Id = id,
                ContentType = contentType,
                EntryKeyHash = crypto.Hash(key),
                EncryptedFileEntry = new FileEntry
                {
                    LabelBytes = crypto.Encrypt(fileEntry.LabelBytes, key, out byte[] labelIV),
                    Data = crypto.Encrypt(fileEntry.Data, key, out byte[] dataIV)
                },
                LabelIV = labelIV,
                DataIV = dataIV,
                LabelLength = fileEntry.LabelBytes.Length,
                DataLength = fileEntry.Data.Length
            };
        }

        public static FileEntry DecryptFileEntry(Crypto crypto, StorageEntry storageEntry, byte[] key)
        {
            return new FileEntry()
            {
                LabelBytes = crypto.Decrypt(storageEntry.EncryptedFileEntry.LabelBytes, key, storageEntry.LabelIV, storageEntry.LabelLength),
                Data = crypto.Decrypt(storageEntry.EncryptedFileEntry.Data, key, storageEntry.DataIV, storageEntry.DataLength)
            };
        }
    }
}
