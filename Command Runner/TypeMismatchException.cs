namespace GSR.CommandRunner
{
	[Serializable]
	public class TypeMismatchException : Exception
	{
		public TypeMismatchException() { }
		public TypeMismatchException(string message) : base(message) { }
		public TypeMismatchException(string message, Exception inner) : base(message, inner) { }
		protected TypeMismatchException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	} // end class
} // end namespace