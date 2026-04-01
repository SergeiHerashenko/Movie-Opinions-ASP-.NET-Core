using Authorization.Application.Exceptions.BaseException;

namespace Authorization.Application.Exceptions
{
    public class ConflictException : BaseApplicationException
    {
        public ConflictException(string errorCode, string message)
            : base(errorCode, message) { }
    }
}
