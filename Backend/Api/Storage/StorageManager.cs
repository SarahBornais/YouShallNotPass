using Aornis;
using System.Runtime.CompilerServices;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Cryptography;

namespace YouShallNotPassBackend.Storage
{
    public class StorageManager : IStorageManager
    {
        private readonly Dictionary<Guid, EntryMetadata> entryMetadata = new();

        private readonly Storage storage;
        private readonly Crypto crypto;

        public StorageManager(Storage storage, Crypto crypto)
        {
            this.storage = storage;
            this.crypto = crypto;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ContentKey AddEntry(Content content)
        {
            ContentKey contentKey = ContentKey.GenerateRandom();

            StorageEntry storageEntry = FileEntry.EncryptFileEntry(
                crypto,
                new(content.Label, content.Data), 
                contentKey.KeyBytes(), 
                content.ContentType, 
                contentKey.Id);

            storage.Write(storageEntry);
            entryMetadata.Add(contentKey.Id, new EntryMetadata(content.ExpirationDate, content.MaxAccessCount));

            return contentKey;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Content GetEntry(ContentKey contentKey)
        {
            Optional<StorageEntry> optionalStorageEntry = storage.Read(contentKey.Id);
            Optional<Exception> exception = CheckEntryForAccess(contentKey, optionalStorageEntry);
            if (exception.HasValue)
            {
                throw exception.Value;
            }

            StorageEntry storageEntry = optionalStorageEntry.Value;
            FileEntry fileEntry = FileEntry.DecryptFileEntry(crypto, storageEntry, contentKey.KeyBytes());

            EntryMetadata metadata = entryMetadata[contentKey.Id];
            metadata.IncrementTimesAccessed();

            return new Content
            {
                ContentType = storageEntry.ContentType, 
                Label = fileEntry.Label,
                ExpirationDate = metadata.ExpirationDate, 
                MaxAccessCount = metadata.MaxAccessCount, 
                TimesAccessed = metadata.TimesAccessed, 
                Data = fileEntry.Data
            };
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool DeleteEntry(Guid id)
        {
            if (!entryMetadata.ContainsKey(id))
            {
                return false;
            }

            entryMetadata.Remove(id);
            storage.Delete(id);

            return true;
        }

        private Optional<Exception> CheckEntryForAccess(ContentKey contentKey, Optional<StorageEntry> optionalStorageEntry)
        {
            if (entryMetadata.TryGetValue(contentKey.Id, out EntryMetadata? metadata))
            {
                if (metadata.ExpirationDate < DateTime.Now || metadata.TimesAccessed >= metadata.MaxAccessCount)
                {
                    return new EntryExpiredException();
                }
            }
            else
            {
                return new EntryNotFoundException();
            }

            if (optionalStorageEntry.HasValue)
            {
                StorageEntry storageEntry = optionalStorageEntry.Value;

                if (!Enumerable.SequenceEqual(crypto.Hash(contentKey.KeyBytes()), storageEntry.EntryKeyHash))
                {
                    return new InvalidKeyException();
                }
            }
            else
            {
                return new EntryNotFoundException();
            }

            return Optional.Empty;
        }
    }
}
