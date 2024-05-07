namespace GSR.CommandRunner
{
    public class Command : ICommand
    {
        public string Name { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        
        private readonly Func<object?[], object?> m_function;



        public Command(string name, Type returnType, Type[] parameterTypes, Func<object?[], object?> func)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;

            m_function = func;
        } // end constructor



        public object? Execute(object?[] parameters)
        {
            if (ParameterTypes.Length != parameters.Length)
                throw new InvalidOperationException($"Expected {ParameterTypes.Length} arguements, was given {parameters.Length}");

            for (int i = 0; i < ParameterTypes.Length; i++)
                if (!ParameterTypes[i].IsAssignableFrom(parameters[i]?.GetType()))
                    throw new InvalidOperationException($"Type error at argument {i + 1}:\n\r\t Expected {ParameterTypes[i]} or subtype, got {parameters[i]?.GetType()}");

            object? result = m_function.Invoke(parameters);
            if (result != null && !ReturnType.IsAssignableFrom(result?.GetType()))
                throw new InvalidOperationException($"Invalid command function return type. Expected {ReturnType} or subtype,  got {result?.GetType()}");

            return result;
        } // end Execute()

    } // end class
} // end namespace