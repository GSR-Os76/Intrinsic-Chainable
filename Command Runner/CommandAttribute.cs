namespace GSR.CommandRunner
{
    /// <summary>
    /// Attribute to mark that a method is a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {

    } // end class
} // end namespace