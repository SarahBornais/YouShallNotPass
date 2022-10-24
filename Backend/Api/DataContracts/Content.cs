using System.Runtime.Serialization;

namespace YouShallNotPassBackend.DataContracts
{
    [DataContract]
    public class Content
    {
        [DataMember(IsRequired = true)]
        public ContentType ContentType { get; set; }

        [DataMember(IsRequired = true)]
        public string? Label { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime ExpirationDate { get; set; }

        [DataMember(IsRequired = true)]
        public int MaxAccessCount { get; set; }

        [DataMember(IsRequired = false)]
        public int TimesAccessed { get; set; }

        [DataMember(IsRequired = true)]
        public byte[]? Data { get; set; }
    }
}
