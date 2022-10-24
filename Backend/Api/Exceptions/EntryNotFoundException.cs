namespace YouShallNotPassBackend.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException() : base() { }
        public EntryNotFoundException(string m) : base(m) { }
    }
}
