namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Exception for when a numeric's value exceeded the max or min bounds of it's type.
    /// </summary>
    [Serializable]
    public class NumericOverflowException : InterpreterException
    {
        /// <inheritdoc/>
        public NumericOverflowException() { }
        /// <inheritdoc/>
        public NumericOverflowException(string message) : base(message) { }
        /// <inheritdoc/>
        public NumericOverflowException(string message, Exception inner) : base(message, inner) { }
        /// <inheritdoc/>
        protected NumericOverflowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace