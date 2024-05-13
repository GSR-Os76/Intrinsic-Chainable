namespace GSR.CommandRunner
{
    /// <summary>
    /// An ICommand implementation for manually providing each part of a command.
    /// </summary>
    public class Command : ICommand
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public Type ReturnType { get; }

        /// <inheritdoc/>
        public Type[] ParameterTypes { get; }


        private readonly Func<object?[], object?> m_function;



        /// <summary>
        /// Construct a command with the provided values.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="func">The behavior to perform when the command is executed. If return type should be void return null instead.</param>
        public Command(string name, Type returnType, Type[] parameterTypes, Func<object?[], object?> func)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;

            m_function = func;
        } // end constructor



        /// <inheritdoc/>
        public object? Execute(object?[] parameters)
        {
            if (ParameterTypes.Length != parameters.Length)
                throw new InvalidCommandOperationException($"Expected {ParameterTypes.Length} arguments, was given {parameters.Length}");

            for (int i = 0; i < ParameterTypes.Length; i++)
                if (!ParameterTypes[i].IsAssignableFrom(parameters[i]?.GetType()))
                    throw new TypeMismatchException($"Type error at argument {i + 1}:\n\r\t Expected {ParameterTypes[i]} or subtype, got {parameters[i]?.GetType()}");

            object? result = m_function.Invoke(parameters);
            if (result != null && !ReturnType.IsAssignableFrom(result?.GetType()))
                throw new TypeMismatchException($"Invalid command function return type. Expected {ReturnType} or subtype,  got {result?.GetType()}");

            return result;
        } // end Execute()

    } // end class
} // end namespace