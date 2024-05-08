namespace GSR.CommandRunner
{
    public interface ISessionContext
    {
        public IList<Variable> Variables { get; }

        public object? GetValue(string name, Type type);

        public void SetValue(string name, object? value);

    } // end class
} // end namespace