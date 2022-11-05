using System.Runtime.Serialization;
using YouShallNotPassBackend.DataContracts;

namespace YouShallNotPassBackend.Storage
{
    [DataContract]
    public class EntryMetadata
    {
        [DataMember(IsRequired = true)]
        public ContentType ContentType { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime ExpirationDate { get; set; }

        [DataMember(IsRequired = true)]
        public int MaxAccessCount { get; set; }

        [DataMember(IsRequired = true)]
        public int TimesAccessed { get; set; } = 0;

        public void IncrementTimesAccessed()
        {
            TimesAccessed++;
        }

        public bool IsEntryExpired()
        {
            return ExpirationDate < DateTime.Now || TimesAccessed >= MaxAccessCount;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EntryMetadata other) return false;

            return ContentType == other.ContentType &&
                ExpirationDate.ToUniversalTime().Equals(other.ExpirationDate.ToUniversalTime()) &&
                MaxAccessCount == other.MaxAccessCount &&
                TimesAccessed == other.TimesAccessed;
        }

        public override int GetHashCode()
        {
            return ContentType.GetHashCode() +
                ExpirationDate.GetHashCode() +
                MaxAccessCount.GetHashCode() +
                TimesAccessed.GetHashCode();
        }
    }
}
