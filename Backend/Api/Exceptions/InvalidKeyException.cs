namespace YouShallNotPassBackend.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException() : base() { }
        public InvalidKeyException(string m) : base(m) { }
    }
}
