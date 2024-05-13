namespace GSR.CommandRunner
{
    [Serializable]
    public class NumericOverflowException : InterpreterException
    {
        public NumericOverflowException() { }
        public NumericOverflowException(string message) : base(message) { }
        public NumericOverflowException(string message, Exception inner) : base(message, inner) { }
        protected NumericOverflowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace