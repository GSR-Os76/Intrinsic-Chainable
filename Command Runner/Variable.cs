namespace GSR.CommandRunner
{
    public class Variable
    {
        public string Name { get; }

#warning possibly remove type, in this case we have the value and so can extract it from that, unlike with parameters and returns
        public Type? Type { get; private set; }

        public object? Value { get; private set; }



        public Variable(string name, Type? type, object? value)
        {
            TypeCheck(type, value);
            Name = name;
            Value = value;
            Type = type;
        } // end constructor



        public void Update(Type? type, object? value) 
        {
            TypeCheck(type, value);
            Type = type;
            Value = value;
        } // end Update()



        private static void TypeCheck(Type? type, object? value)
        {
            bool eitherNull = (type == null || value == null);
            if ((eitherNull && type != value?.GetType()) || (!eitherNull && !type.IsAssignableFrom(value?.GetType())))
                throw new TypeMismatchException($"Variable type mismatch, got: {type}, expected: {value?.GetType()}");
        } // end NullCheck

    } // end class
} // end namespace
