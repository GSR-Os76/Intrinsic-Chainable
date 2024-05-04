namespace GSR.CommandRunner
{
    public interface ICommand
    {
        public string Code { get; }

        public Type ReturnType { get; }

        public Type[] Parameters { get; }



        public object? Execute(object[] parameters);

    } // end class
} // end namespace