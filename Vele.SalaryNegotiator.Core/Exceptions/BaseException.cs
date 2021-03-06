using System;
using System.Runtime.Serialization;

namespace Vele.SalaryNegotiator.Core.Exceptions
{

    [Serializable]
    public abstract class BaseException : Exception
    {
        public BaseException(string message) : base(message) { }

        public BaseException(string message, Exception inner) : base(message, inner) { }

        protected BaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
