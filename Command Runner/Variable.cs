namespace GSR.CommandRunner
{
    public class Variable
    {
        public string Name { get; }

        public object? Value { get; set; }



        public Variable(string name, object? value)
        {
            Name = name;
            Value = value;
        } // end constructor

    } // end class
} // end namespace
