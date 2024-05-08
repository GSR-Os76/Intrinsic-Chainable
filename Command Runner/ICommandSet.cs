namespace GSR.CommandRunner
{
    public interface ICommandSet
    {
        public IList<ICommand> Commands { get; }

        ICommand GetCommand(string name, int paramCount);
    } // end class
} // end namespace
