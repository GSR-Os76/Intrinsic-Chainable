using System.Collections.Immutable;
using System.Reflection;

namespace GSR.CommandRunner
{
    public class CommandSet : ICommandSet
    {
        public IList<ICommand> Commands { get; }



        public CommandSet(Type commandSource) : this(new List<Type>() { commandSource }) { } // end constructor

        public CommandSet(IEnumerable<Type> commandSources) : this(commandSources
            .SelectMany((x) => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where((x) => x.GetCustomAttribute(typeof(CommandAttribute)) is not null)) { } // end constructor


        public CommandSet(IEnumerable<MethodInfo> commandSources) 
        {
            Commands = commandSources.Select((x) => (ICommand)new MethodInfoCommand(x)).ToImmutableList();

            IEnumerable<ICommand> collisions = Commands.Where((x) => Commands.Any((y) => !ReferenceEquals(x, y) && x.Code.Equals(y.Code)));
            if (collisions.Any())
                throw new ArgumentException($"Encountered command collisions: {collisions.Aggregate("", (x, y) => x + $"\n\r\tCollision for signature: {y}")}");
        } // end constructors

    } // end class
} // end namespace
