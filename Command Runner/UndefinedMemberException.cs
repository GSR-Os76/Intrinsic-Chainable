namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Exception for when a member(a variable or method) failed to be located.
    /// </summary>
    [Serializable]
    public class UndefinedMemberException : InterpreterException
    {
        /// <inheritdoc/>
        public UndefinedMemberException() { }
        /// <inheritdoc/>
        public UndefinedMemberException(string message) : base(message) { }
        /// <inheritdoc/>
        public UndefinedMemberException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected UndefinedMemberException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace