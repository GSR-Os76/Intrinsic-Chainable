namespace GSR.CommandRunner
{

	[Serializable]
	public class InvalidStateException : Exception
	{
		public InvalidStateException() { }
		public InvalidStateException(string message) : base(message) { }
		public InvalidStateException(string message, Exception inner) : base(message, inner) { }
		protected InvalidStateException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	} // end class
} // end namespace