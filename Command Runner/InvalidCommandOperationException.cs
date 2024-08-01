namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Generic exception for interpretation errors.
    /// </summary>
    [Serializable]
    public class InvalidCommandOperationException : InterpreterException
    {
        /// <inheritdoc/>
        public InvalidCommandOperationException() { }
        /// <inheritdoc/>
        public InvalidCommandOperationException(string message) : base(message) { }
        /// <inheritdoc/>
        public InvalidCommandOperationException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected InvalidCommandOperationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace