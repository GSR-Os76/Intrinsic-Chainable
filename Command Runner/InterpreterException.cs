namespace GSR.CommandRunner
{
    [Serializable]
    public class InterpreterException : Exception
    {
        public InterpreterException() { }
        public InterpreterException(string message) : base(message) { }
        public InterpreterException(string message, Exception inner) : base(message, inner) { }
        protected InterpreterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace