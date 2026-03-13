namespace Authorization.Domain.Exceptions
{
    public class ReturningNoDataException : BaseApplicationException
    {
        public string Operation { get; }

        public ReturningNoDataException(string operation)
            : base($"{operation}", 500)
        {
            Operation = operation;
        }
    }
}
