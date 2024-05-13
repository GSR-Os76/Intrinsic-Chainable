namespace GSR.CommandRunner
{
    /// <summary>
    /// Exception stating the program somehow reached an invalid state. Generally reserved for theoretically unreachable code paths that still need a return value, and is expected not to be thrown.
    /// </summary>
    [Serializable]
    internal class InvalidStateException : Exception
    {
        /// <inheritdoc/>
        public InvalidStateException() { }
        /// <inheritdoc/>
        public InvalidStateException(string message) : base(message) { }
        /// <inheritdoc/>
        public InvalidStateException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected InvalidStateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace