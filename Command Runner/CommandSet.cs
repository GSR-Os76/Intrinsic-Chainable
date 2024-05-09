using System.Collections.Immutable;
using System.Reflection;

namespace GSR.CommandRunner
{
    public class CommandSet : ICommandSet
    {
        public IList<ICommand> Commands { get; }



        public CommandSet() : this(new List<MethodInfo>()){ } // end constructor

        public CommandSet(Type commandSource) : this(new List<Type>() { commandSource }) { } // end constructor

        public CommandSet(IEnumerable<Type> commandSources) : this(commandSources
            .SelectMany((x) => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where((x) => x.GetCustomAttribute(typeof(CommandAttribute)) is not null)) { } // end constructor


        public CommandSet(IEnumerable<MethodInfo> commandSources) 
        {
            Commands = commandSources.Select((x) => (ICommand)new MethodInfoCommand(x)).ToImmutableList();

            IEnumerable<ICommand> collisions = Commands.Where((x) => Commands.Any((y) => !ReferenceEquals(x, y) && x.Name.Equals(y.Name) && x.ParameterTypes.Length == y.ParameterTypes.Length));
            if (collisions.Any())
                throw new ArgumentException($"Encountered command collisions: {collisions.Aggregate("", (x, y) => x + $"\n\r\tCollision for signature: {y}")}");
        } // end constructors

        public ICommand GetCommand(string name, int paramCount)
        {
            IEnumerable<ICommand> matches = Commands.Where((x) => name.Equals(x.Name) && paramCount == x.ParameterTypes.Length);
            if (!matches.Any())
                throw new UndefinedMemberException($"No such command \"{name}\" with \"{paramCount}\" parameters was found.");
            else if (matches.Count() > 1)
                throw new InvalidStateException($"multiple such command \"{name}\" with \"{paramCount}\" parameters was found, this shouldn't happen.");
            else
                return matches.First();
        } // end GetCommand()

    } // end class
} // end namespace
