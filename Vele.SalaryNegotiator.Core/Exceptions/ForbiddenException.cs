using System;
using System.Runtime.Serialization;

namespace Vele.SalaryNegotiator.Core.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException() : this("Forbidden") { }

        public ForbiddenException(string message) : base(message) { }

        public ForbiddenException(string message, Exception inner) : base(message, inner) { }

        protected ForbiddenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
