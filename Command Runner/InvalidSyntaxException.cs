namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Exception indicating the interpreter met an unexpected character or lack of a character.
    /// </summary>
    [Serializable]
    public class InvalidSyntaxException : InterpreterException
    {
        /// <inheritdoc/>
        public InvalidSyntaxException() { }
        /// <inheritdoc/>
        public InvalidSyntaxException(string message) : base(message) { }
        /// <inheritdoc/>
        public InvalidSyntaxException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected InvalidSyntaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace