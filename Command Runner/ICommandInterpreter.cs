namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// The contract for an object which can convert command code into equivalent ICommands.
    /// </summary>
    public interface ICommandInterpreter
    {
        /// <summary>
        /// Evaluate a provided command in code form.  
        /// </summary>
        /// <param name="input">The command code. See ~Help() for command rules.</param>
        /// <returns>A command that performs the code provided. This included assignments, and retrieving values. The command must be executed to retrieve a value, or perform actions.</returns>
        public ICommand Evaluate(string input);

    } // end class
} // end namespace