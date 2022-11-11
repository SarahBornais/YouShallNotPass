using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Security
{
    public interface IAuthenticator
    {
        bool Authenticate(Service service);
    }
}
