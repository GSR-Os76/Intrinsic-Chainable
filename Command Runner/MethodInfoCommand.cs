using System.Reflection;

namespace GSR.CommandRunner
{
    public class MethodInfoCommand : Command
    {
        public MethodInfoCommand(MethodInfo from) : base(from.Name, from.ReturnType, from.GetParameters().Select((x) => x.ParameterType).ToArray(), (p) => from.Invoke(null, p)) { } // end constructor

    } // end class
} // end namespace