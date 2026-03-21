namespace Profile.Domain.Exceptions
{
    public class DatabaseOperationException : BaseApplicationException
    {
        public string SqlState { get; }

        public DatabaseOperationException(string message, string sqlState)
            : base(message, 500)
        {
            SqlState = sqlState;
        }
    }
}
