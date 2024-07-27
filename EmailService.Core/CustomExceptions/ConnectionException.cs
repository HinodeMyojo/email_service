namespace EmailService.Core.CustomExceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }
}
