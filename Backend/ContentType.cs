using System.Runtime.Serialization;

namespace YouShallNotPassBackend
{
    [DataContract]
    public enum ContentType
    {
        PDF,
        PNG,
        TEXT
    }
}
