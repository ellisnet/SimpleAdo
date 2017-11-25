using System;

namespace SimpleAdo.Common
{
    /// <summary> Exception for signalling database errors. </summary>
    public abstract class DbException : Exception
    {
        /// <summary> Specialised default constructor for use only by derived class. </summary>
        protected DbException()
        {
        }

        /// <summary> Specialised constructor for use only by derived class. </summary>
        /// <param name="message"> The message. </param>
        protected DbException(string message)
            : base(message)
        {
        }

        /// <summary> Specialised constructor for use only by derived class. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="innerException"> The inner exception. </param>
        protected DbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary> Specialised constructor for use only by derived class. </summary>
        /// <param name="message"> The message. </param>
        /// <param name="errorCode"> The error code. </param>
        protected DbException(string message, int errorCode)
            : base(message)
        {
            HResult = errorCode;
        }

        /// <summary> Gets or sets the error code. </summary>
        /// <value> The error code. </value>
        public int ErrorCode => HResult;
    }

}
