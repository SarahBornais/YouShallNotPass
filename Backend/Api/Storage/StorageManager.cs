using Aornis;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Security;
using Timer = System.Timers.Timer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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

            EntryMetadata entryMetadata = new()
            {
                Id = contentKey.Id,
                EntryKeyHash = Crypto.Hash(contentKey.KeyBytes()),
                ContentType = content.ContentType,
                ExpirationDate = content.ExpirationDate,
                MaxAccessCount = content.MaxAccessCount
            };

            StorageEntry storageEntry = new()
            {
                Metadata = entryMetadata,
                Data = crypto.Encrypt(content.Data, contentKey.KeyBytes()),
                Label = crypto.EncryptString(content.Label, contentKey.KeyBytes())
            };

            storage.Write(storageEntry); 

            return contentKey;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Content GetEntry(ContentKey contentKey)
        {
            StorageEntry storageEntry = GetAndCheckForAccess(contentKey, storage.Read(contentKey.Id));
            storage.Write(storageEntry.IncrementTimesAccessed());

            return new Content
            {
                ContentType = storageEntry.Metadata.ContentType, 
                Label = crypto.DecryptToString(storageEntry.Label, contentKey.KeyBytes()),
                ExpirationDate = storageEntry.Metadata.ExpirationDate, 
                MaxAccessCount = storageEntry.Metadata.MaxAccessCount, 
                TimesAccessed = storageEntry.Metadata.TimesAccessed, 
                Data = crypto.Decrypt(storageEntry.Data, contentKey.KeyBytes())
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
                if (storageEntry.HasValue && storageEntry.Value.Metadata.IsEntryExpired())
                {
                    DeleteEntry(id);
                }
            }
        }

        private StorageEntry GetAndCheckForAccess(ContentKey contentKey, Optional<StorageEntry> storageEntry)
        {
            if (storageEntry.HasValue)
            {
                if (storageEntry.Value.Metadata.IsEntryExpired())
                {
                    throw new EntryExpiredException();
                }

                if (!Enumerable.SequenceEqual(Crypto.Hash(contentKey.KeyBytes()), storageEntry.Value.Metadata.EntryKeyHash.Bytes))
                {
                    throw new InvalidKeyException();
                }
            }
            else
            {
                throw new EntryNotFoundException();
            }

            return storageEntry.Value;
        }
    }
}
