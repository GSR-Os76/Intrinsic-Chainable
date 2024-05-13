namespace GSR.CommandRunner
{
    /// <summary>
    /// Base class of any exception that is the result of a problem interpreting command code.
    /// </summary>
    [Serializable]
    public class InterpreterException : Exception
    {
        /// <inheritdoc/>
        public InterpreterException() { }
        /// <inheritdoc/>
        public InterpreterException(string message) : base(message) { }
        /// <inheritdoc/>
        public InterpreterException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected InterpreterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace