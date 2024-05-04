using System.Collections.Immutable;
using System.Reflection;

namespace GSR.CommandRunner
{
    public class CommandSet : ICommandSet
    {
        public IList<ICommand> Commands { get; }



        public CommandSet(IEnumerable<Type> commandSources) : this(commandSources
            .SelectMany((x) => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where((x) => x.GetCustomAttribute(typeof(CommandAttribute)) is not null)) { } // end constructor


        public CommandSet(IEnumerable<MethodInfo> commandSources) 
        {
            Commands = commandSources.Select((x) => (ICommand)new MethodInfoCommand(x)).ToImmutableList();
        } // end constructors

    } // end class
} // end namespace
