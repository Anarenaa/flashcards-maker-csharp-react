namespace Core.Exceptions
{
    public class BadHttpRequestException : Exception
    {
        public int StatusCode { get; }

        public BadHttpRequestException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
