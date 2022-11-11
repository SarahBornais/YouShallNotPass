using Aornis;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Security;
using Timer = System.Timers.Timer;

namespace YouShallNotPassBackend.Storage
{
    public class StorageManager : IStorageManager
    {
        private readonly Storage storage;
        private readonly Crypto crypto;

        public StorageManager(Storage storage, Crypto crypto, int? clearingInvertalMillis)
        {
            this.storage = storage;
            this.crypto = crypto;

            if (clearingInvertalMillis != null)
            {
                BackgroundWorker worker = new();
                worker.DoWork += (_, _) => RemoveExpiredEntries();

                Timer timer = new(clearingInvertalMillis.Value);
                timer.Elapsed += (_, _) =>
                {
                    if (!worker.IsBusy) worker.RunWorkerAsync();
                };
                timer.Start();
            }
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
            AssertCanAccess(contentKey, optionalStorageEntry);

            StorageEntry storageEntry = optionalStorageEntry.Value;
            storageEntry.EntryMetadata.IncrementTimesAccessed();
            storage.Write(storageEntry);

            FileEntry fileEntry = FileEntry.DecryptFileEntry(crypto, storageEntry, contentKey.KeyBytes());

            return new Content
            {
                ContentType = storageEntry.EntryMetadata.ContentType, 
                Label = fileEntry.Label,
                ExpirationDate = storageEntry.EntryMetadata.ExpirationDate, 
                MaxAccessCount = storageEntry.EntryMetadata.MaxAccessCount, 
                TimesAccessed = storageEntry.EntryMetadata.TimesAccessed, 
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

        private void AssertCanAccess(ContentKey contentKey, Optional<StorageEntry> storageEntry)
        {
            if (storageEntry.HasValue)
            {
                if (storageEntry.Value.EntryMetadata.IsEntryExpired())
                {
                    throw new EntryExpiredException();
                }

                if (!Enumerable.SequenceEqual(Crypto.Hash(contentKey.KeyBytes()), storageEntry.Value.EntryKeyHash))
                {
                    throw new InvalidKeyException();
                }
            }
            else
            {
                throw new EntryNotFoundException();
            }
        }
    }
}
