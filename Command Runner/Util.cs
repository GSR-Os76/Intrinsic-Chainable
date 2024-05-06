namespace GSR.CommandRunner
{
    public static class Util
    {
        public static object? Execute(this ICommand c) => c.Execute(Array.Empty<object?>());
    } // end class
} // end namespace