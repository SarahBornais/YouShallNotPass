using System.Runtime.Serialization;

namespace YouShallNotPassBackend.DataContracts
{
    [DataContract]
    public enum ContentType
    {
        PDF,
        PNG,
        TEXT
    }
}
