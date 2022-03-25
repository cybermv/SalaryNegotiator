using System;
using System.Runtime.Serialization;

namespace Vele.SalaryNegotiator.Core.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException() : this("Not found") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception inner) : base(message, inner) { }

        protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
