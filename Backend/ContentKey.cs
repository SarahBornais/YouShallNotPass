using System.Runtime.Serialization;

namespace YouShallNotPassBackend
{
    [DataContract]
    public class ContentKey
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Key { get; set; } = "";
    }
}
