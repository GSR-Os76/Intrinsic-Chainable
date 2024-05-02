namespace GSR.CommandRunner
{
    public interface ISessionContext
    {

        public object GetValue(string name, Type type);

        public void SetValue(string name, object value);

    } // end class
} // end namespace