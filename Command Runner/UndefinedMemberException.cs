namespace GSR.CommandRunner
{
    [Serializable]
    public class UndefinedMemberException : InterpreterException
    {
        public UndefinedMemberException() { }
        public UndefinedMemberException(string message) : base(message) { }
        public UndefinedMemberException(string message, Exception inner) : base(message, inner) { }
        protected UndefinedMemberException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    } // end class
} // end namespace