namespace GSR.CommandRunner
{
    [Serializable]
    public class InvalidCommandOperationException : InterpreterException
    {
        public InvalidCommandOperationException() { }
        public InvalidCommandOperationException(string message) : base(message) { }
        public InvalidCommandOperationException(string message, Exception inner) : base(message, inner) { }
        protected InvalidCommandOperationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace