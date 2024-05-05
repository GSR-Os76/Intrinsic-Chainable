namespace GSR.CommandRunner
{
    public interface ICommand
    {
        public string Code { get; }

        public string Name { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }



        public object? Execute(object[] parameters);

    } // end class
} // end namespace