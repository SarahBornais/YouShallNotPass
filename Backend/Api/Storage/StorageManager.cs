using Aornis;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Cryptography;
using Timer = System.Timers.Timer;
using System.Data;

namespace YouShallNotPassBackend.Storage
{
    public class StorageManager : IStorageManager
    {
        private readonly Storage storage;
        private readonly Crypto crypto;
        private readonly BackgroundWorker worker;

        public StorageManager(Storage storage, Crypto crypto, int clearingInvertalMillis)
        {
            this.storage = storage;
            this.crypto = crypto;

            worker = new();
            worker.DoWork += (_, _) => RemoveExpiredEntries();

            Timer timer = new(clearingInvertalMillis);
            timer.Elapsed += (_, _) =>
            {
                if (!worker.IsBusy) worker.RunWorkerAsync();
            };
            timer.Start();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ContentKey AddEntry(Content content)
        {
            ContentKey contentKey = ContentKey.GenerateRandom();

            EntryMetadata entryMetaData = new()
            {
                ContentType = content.ContentType,
                ExpirationDate = content.ExpirationDate,
                MaxAccessCount = content.MaxAccessCount
            };

            StorageEntry storageEntry = FileEntry.EncryptFileEntry(
                crypto,
                new(content.Label, content.Data), 
                contentKey.KeyBytes(),
                entryMetaData, 
                contentKey.Id);

            storage.Write(storageEntry); 

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

            EntryMetadata metadata = storageEntry.EntryMetadata;
            metadata.IncrementTimesAccessed();
            storage.Write(storageEntry);

            return new Content
            {
                ContentType = storageEntry.EntryMetadata.ContentType, 
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
            if (!storage.Contains(id))
            {
                return false;
            }

            storage.Delete(id);
            return true;
        }

        public void Clear()
        {
            storage.Clear();
        }

        public void RemoveExpiredEntries()
        {
            foreach (Guid id in storage.GetAllEntryGuids())
            {
                Optional<StorageEntry> storageEntry = storage.Read(id);
                if (storageEntry.HasValue && storageEntry.Value.EntryMetadata.IsEntryExpired())
                {
                    DeleteEntry(id);
                }
            }
        }

        private Optional<Exception> CheckEntryForAccess(ContentKey contentKey, Optional<StorageEntry> storageEntry)
        {
            if (storageEntry.HasValue)
            {
                if (storageEntry.Value.EntryMetadata.IsEntryExpired())
                {
                    return new EntryExpiredException();
                }

                if (!Enumerable.SequenceEqual(crypto.Hash(contentKey.KeyBytes()), storageEntry.Value.EntryKeyHash))
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
