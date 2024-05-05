namespace GSR.CommandRunner
{
    public interface ICommandInterpreter
    {
        public ICommand Evaluate(string input);

    } // end class
} // end namespace