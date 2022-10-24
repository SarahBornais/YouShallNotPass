namespace YouShallNotPassBackend.Storage
{
    public class EntryMetadata
    {
        public EntryMetadata(DateTime expirationDate, int maxAccessCount)
        {
            ExpirationDate = expirationDate;
            MaxAccessCount = maxAccessCount;
            TimesAccessed = 0;
        }

        public DateTime ExpirationDate { get; }

        public int MaxAccessCount { get; }

        public int TimesAccessed { get; private set; }

        public void IncrementTimesAccessed()
        {
            TimesAccessed++;
        }
    }
}
