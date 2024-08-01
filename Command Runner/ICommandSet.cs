namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Contract for an object which can hold a set of commands.
    /// </summary>
    public interface ICommandSet
    {
        /// <summary>
        /// A list of all commands that're stored in the set. Should not be used to modify them.
        /// </summary>
        public IList<ICommand> Commands { get; }

        /// <summary>
        /// Get's a command with a given signature.
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <param name="paramCount">The number of parameter the command has.</param>
        /// <returns>The identified command.</returns>
        /// <exception cref="UndefinedMemberException">No command exists with the given name and number of parameters.</exception>
        ICommand GetCommand(string name, int paramCount);
    } // end class
} // end namespace
