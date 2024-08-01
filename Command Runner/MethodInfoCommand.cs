using System.Reflection;

namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// An ICommand implementation for creating a command that's equivalent to an MethodInfo instance.
    /// </summary>
    public class MethodInfoCommand : Command
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">The MethodInfo that the command will mirror, taking it's name, return/parametere types, and it's behavior.</param>
        public MethodInfoCommand(MethodInfo from) : base(from.Name, from.ReturnType, from.GetParameters().Select((x) => x.ParameterType).ToArray(), (p) => from.Invoke(null, p)) { } // end constructor

    } // end class
} // end namespace