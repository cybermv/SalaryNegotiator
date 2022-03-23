using FluentValidation.Results;
using System;
using System.Runtime.Serialization;

namespace Vele.SalaryNegotiator.Core.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationResult ValidationResult { get; }

        public ValidationException() { }

        public ValidationException(string message) : base(message) { }

        public ValidationException(string message, Exception inner) : base(message, inner) { }

        public ValidationException(ValidationResult result) : base("Validation errors")
        {
            ValidationResult = result;
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
