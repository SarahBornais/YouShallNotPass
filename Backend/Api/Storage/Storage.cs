using Aornis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using YouShallNotPassBackend.Exceptions;

namespace YouShallNotPassBackend.Storage
{
    public class Storage
    {
        private readonly string storageDirectory;

        public Storage(string storageDirectory)
        {
            this.storageDirectory = storageDirectory;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Write(StorageEntry storageEntry)
        {
            string fileLocation = GetFileLocation(storageEntry.Id);

            Stream stream = File.Open(fileLocation, FileMode.Create);
            JsonSerializer.Serialize(stream, storageEntry);
            stream.Close();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Optional<StorageEntry> Read(Guid id)
        {
            string fileLocation = GetFileLocation(id);

            if (!File.Exists(fileLocation))
            {
                return Optional.Empty;
            }

            Stream stream = File.Open(fileLocation, FileMode.Open);
            StorageEntry? storageEntry = JsonSerializer.Deserialize<StorageEntry>(stream);
            stream.Close();

            if (storageEntry == null)
            {
                throw new InvalidOperationException($"{storageEntry} is null");
            }

            return storageEntry;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Delete(Guid id)
        {
            File.Delete(GetFileLocation(id));
        }

        private string GetFileLocation(Guid id)
        {
            return Path.Combine(storageDirectory, id.ToString());
        }
    }
}
