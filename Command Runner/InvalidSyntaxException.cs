namespace GSR.CommandRunner
{
    [Serializable]
    public class InvalidSyntaxException : InterpreterException
    {
        public InvalidSyntaxException() { }
        public InvalidSyntaxException(string message) : base(message) { }
        public InvalidSyntaxException(string message, Exception inner) : base(message, inner) { }
        protected InvalidSyntaxException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace