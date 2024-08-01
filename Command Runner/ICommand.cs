namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Signature for commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The commands name, used along with parameter count to identify it within an ICommandSet, and for use identifying it within command interpretation. Should be static.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type the command returns. Should be static.
        /// </summary>
        public Type ReturnType { get; }

        /// <summary>
        /// In expected order the expected type of each parameter. Should be static.
        /// </summary>
        public Type[] ParameterTypes { get; }



        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="arguments">The arguments for the command to use, must match the length of the ParameterTypes property's value. Must match the types listed in the ParameterTypes property.</param>
        /// <returns>A value that's matching the type listed in the ReturnType property. Or for any type possibly a null.</returns>
        /// <exception cref="InvalidCommandOperationException">The number of arguments received didn't match the number of parameters.</exception>
        /// <exception cref="TypeMismatchException">An argument type didn't match the expected parameter type.</exception>
        public object? Execute(object?[] arguments);

    } // end class
} // end namespace