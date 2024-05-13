namespace GSR.CommandRunner
{
    /// <summary>
    /// Exception for when the expected type didn't match the received type.
    /// </summary>
    [Serializable]
    public class TypeMismatchException : InterpreterException
    {
        /// <inheritdoc/>
        public TypeMismatchException() { }
        /// <inheritdoc/>
        public TypeMismatchException(string message) : base(message) { }
        /// <inheritdoc/>
        public TypeMismatchException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected TypeMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace