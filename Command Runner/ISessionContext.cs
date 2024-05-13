namespace GSR.CommandRunner
{
    /// <summary>
    /// Signature for an object holding information of the current interpretation session.
    /// </summary>
    public interface ISessionContext
    {
        /// <summary>
        /// Get all current variables in the system. Shouldn't be used to mutate variable values.
        /// </summary>
        public IList<Variable> Variables { get; }



        /// <summary>
        /// Get the value stored in a variable.
        /// </summary>
        /// <param name="name">The variable's name.</param>
        /// <returns>The variable's value.</returns>
        /// <exception cref="UndefinedMemberException">No variable by that name was found.</exception>
        public object? GetValue(string name);

        /// <summary>
        /// Adds or updates a variable by the name <paramref name="name"/> to have the value <paramref name="value"/>.
        /// </summary>
        /// <param name="name">The variable to be updated, or the new variable's name.</param>
        /// <param name="value">The value to store in the variable.</param>
        public void SetValue(string name, object? value);

    } // end class
} // end namespace