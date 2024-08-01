namespace GSR.IntrinsicChainable
{
    /// <summary>
    /// Class containg utility extension method's.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Execute the command without arguments.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static object? Execute(this ICommand c) => c.Execute(Array.Empty<object?>());
    } // end class
} // end namespace