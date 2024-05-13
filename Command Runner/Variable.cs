namespace GSR.CommandRunner
{
    public class Variable
    {
        /// <summary>
        /// The variables scope unique name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value that's stored in the variable.
        /// </summary>
        public object? Value { get; set; }



        public Variable(string name, object? value)
        {
            Name = name;
            Value = value;
        } // end constructor

    } // end class
} // end namespace
