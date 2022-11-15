namespace YouShallNotPassBackend.Exceptions
{
    public class InvalidSecurityQuestionAnswerException : Exception
    {
        public InvalidSecurityQuestionAnswerException() : base() { }
        public InvalidSecurityQuestionAnswerException(string m) : base(m) { }
    }
}
