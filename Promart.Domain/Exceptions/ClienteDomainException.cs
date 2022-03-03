using System;

namespace Promart.Domain.Exceptions
{
    public class ClienteDomainException : Exception
    {
        public ClienteDomainException()
        { }

        public ClienteDomainException(string message)
            : base(message)
        { }

        public ClienteDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
