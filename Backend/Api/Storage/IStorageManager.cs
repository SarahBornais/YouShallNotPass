using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    public interface IStorageManager
    {
        ContentKey AddEntry(Content content);

        Content GetEntry(ContentKey contentKey);

        bool DeleteEntry(Guid id);

        string? GetSecurityQuestion(Guid id);
    }
}
