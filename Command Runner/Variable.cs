namespace GSR.CommandRunner
{
    public class Variable
    {
        public string Name { get; }

        public Type? Type { get; private set; }

        public object? Value { get; private set; }



        public Variable(string name, Type? type, object? value)
        {
            Name = name;
            Value = value;
            Type = type;
#warning, fail fast if type wrong
        } // end constructor



        public void Update(Type? type, object? value) 
        {
            Type = type;
            Value = value;
        } // end Update()

    } // end class
} // end namespace
