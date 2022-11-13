using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    public interface IStorageManager
    {
        public ContentKey AddEntry(Content content);

        public Content GetEntry(ContentKey contentKey);

        public bool DeleteEntry(Guid id);
    }
}
