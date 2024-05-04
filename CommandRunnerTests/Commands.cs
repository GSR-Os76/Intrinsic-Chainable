using GSR.CommandRunner;
using System.Reflection;
using System.Text;

namespace GSR.Tests.CommandRunner
{
    public static class Commands
    {
        public static readonly MethodInfo ABMethod = typeof(Commands).GetMethod(nameof(AB)) ?? throw new NullReferenceException($"Failed to find: {nameof(AB)}");

        public static readonly MethodInfo CommadTetsMethod = typeof(Commands).GetMethod(nameof(CommadTets)) ?? throw new NullReferenceException($"Failed to find: {nameof(CommadTets)}");

        public static readonly MethodInfo smkdlmMethod = typeof(Commands).GetMethod(nameof(smkdlm)) ?? throw new NullReferenceException($"Failed to find: {nameof(smkdlm)}");

        public static readonly MethodInfo SMethod = typeof(Commands).GetMethod(nameof(S)) ?? throw new NullReferenceException($"Failed to find: {nameof(S)}");



        [Command]
        public static void AB() { }

        [Command]
        public static StringBuilder CommadTets(int a, string b) => new StringBuilder(b).Append(" ").Append(a);

        [Command]
        public static int smkdlm(int a) => a;

        [Command]
        public static int S(Exception a) => 0;
    } // end class
} // end namespace