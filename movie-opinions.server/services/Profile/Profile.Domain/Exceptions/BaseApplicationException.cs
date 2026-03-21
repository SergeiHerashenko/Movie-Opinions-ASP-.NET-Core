namespace Profile.Domain.Exceptions
{
    public class BaseApplicationException : Exception
    {
        public int StatusCode { get; }

        protected BaseApplicationException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
