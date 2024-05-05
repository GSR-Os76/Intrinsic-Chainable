namespace GSR.CommandRunner
{
    public interface ICommand
    {
        public string Name { get; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }



        public object? Execute(object?[] parameters);

    } // end class
} // end namespace