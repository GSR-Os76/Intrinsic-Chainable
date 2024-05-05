using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class Command : ICommand
    {
        public string Code { get; }

        public string Name { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        
        private readonly Func<object[], object?> m_function;



        public Command(string name, Type returnType, Type[] parameterTypes, Func<object[], object?> func)
        {
            Name = name;
            Code = $"{name}({Regex.Replace(new string('?', parameterTypes.Length), @"(?<=.)(?!$)", ", ")})";
            ReturnType = returnType;
            ParameterTypes = parameterTypes;

            m_function = func;
        } // end constructor



        public object? Execute(object[] parameters)
        {
            if (ParameterTypes.Length != parameters.Length)
                throw new InvalidOperationException($"Expected {ParameterTypes.Length} arguements, was given {parameters.Length}");

            for (int i = 0; i < ParameterTypes.Length; i++)
                if (!ParameterTypes[i].IsAssignableFrom(parameters[i].GetType()))
                    throw new InvalidOperationException($"Type error at argument {i + 1}:\n\r\t expected {ParameterTypes[i]} or subtype, got {parameters[i].GetType()}");

            return m_function.Invoke(parameters);
        } // end Execute()

    } // end class
} // end namespace