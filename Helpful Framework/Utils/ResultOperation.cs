using System;

namespace Helpful.Framework.Utils
{
    /// <summary>Represents the result of an operation</summary>
    public class ResultOperation
    {
        /// <summary>Whether the operation was successful</summary>
        public bool IsSuccess => Exception == null;
        /// <summary>Whether the operation was unsuccessful</summary>
        public bool IsError => Exception != null;
        /// <summary>The exception of the operation</summary>
        public Exception Exception { get; } = null;

        // Exception defaults to null, therefore no worries
        private ResultOperation() { }
        private ResultOperation(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>Constructs a successful <see cref="ResultOperation"/></summary>
        public static ResultOperation FromSuccess() => new ResultOperation();
        /// <summary>Constructs an unsuccessful <see cref="ResultOperation"/></summary>
        public static ResultOperation FromError(Exception exception) => new ResultOperation(exception);
    }
}
