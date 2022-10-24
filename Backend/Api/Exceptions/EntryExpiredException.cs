namespace YouShallNotPassBackend.Exceptions
{
    public class EntryExpiredException : Exception
    {
        public EntryExpiredException() : base() { }
        public EntryExpiredException(string m) : base(m) { }
    }
}
