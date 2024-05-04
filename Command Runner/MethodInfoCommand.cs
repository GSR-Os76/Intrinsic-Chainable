using System.Reflection;
using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class MethodInfoCommand : ICommand
    {
        public string Code { get; }

        public Type ReturnType { get; }

        public Type[] Parameters { get; }

        private readonly Func<object[], object?> m_function;



        public MethodInfoCommand(MethodInfo from) 
        {
            Code = $"{from.Name}({Regex.Replace(new string('_', from.GetParameters().Length), @"(?<=.)(?!$)", ", ")})";
            ReturnType = from.ReturnType;
            Parameters = from.GetParameters().Select((x) => x.ParameterType).ToArray();

            m_function = (p) => from.Invoke(null, p);
        } // end constructor



        public object? Execute(object[] parameters)
        {
            if(Parameters.Length != parameters.Length)
                throw new InvalidOperationException($"Expected {Parameters.Length} arguements, was given {parameters.Length}");

            for(int i = 0; i < Parameters.Length; i++)
                if (!Parameters[i].IsAssignableFrom(parameters[i].GetType()))
                    throw new InvalidOperationException($"Type error at argument {i +1}:\n\r\t expected {Parameters[i]} or subtype, got {parameters[i].GetType()}");

            return m_function.Invoke(parameters);
        } // end Execute()

    } // end class
} // end namespace