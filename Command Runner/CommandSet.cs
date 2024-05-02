using System.Reflection;

namespace GSR.CommandRunner
{
    public class CommandSet : ICommandSet
    {
        public IList<ICommand> Commands => throw new NotImplementedException();



        public CommandSet()//IEnumerable<Assembly>) 
        {
        
        } // end constructors

    } // end class
} // end namespace
