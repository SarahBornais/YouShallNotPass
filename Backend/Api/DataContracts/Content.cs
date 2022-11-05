using System.Runtime.Serialization;

namespace YouShallNotPassBackend.DataContracts
{
    [DataContract]
    public class Content
    {
        [DataMember(IsRequired = true)]
        public ContentType ContentType { get; set; }

        [DataMember(IsRequired = true)]
        public string Label { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime ExpirationDate { get; set; }

        [DataMember(IsRequired = true)]
        public int MaxAccessCount { get; set; }

        [DataMember(IsRequired = false)]
        public int TimesAccessed { get; set; }

        [DataMember(IsRequired = true)]
        public byte[] Data { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not Content other) return false;

            return ContentType == other.ContentType &&
                Label == other.Label &&
                ExpirationDate == other.ExpirationDate &&
                MaxAccessCount == other.MaxAccessCount &&
                Enumerable.SequenceEqual(Data, other.Data);
        }

        public override int GetHashCode()
        {
            return ContentType.GetHashCode() +
                Label.GetHashCode() +
                ExpirationDate.GetHashCode() +
                MaxAccessCount.GetHashCode() +
                TimesAccessed.GetHashCode() +
                Data.GetHashCode();
        }

    }
}
