using GSR.IntrinsicChainable;
using System.Reflection;
using System.Text;

namespace GSR.Tests.IntrinsicChainable
{
    public static class CommandSetOne
    {
        public static readonly MethodInfo ABMethod = typeof(CommandSetOne).GetMethod(nameof(AB)) ?? throw new NullReferenceException($"Failed to find: {nameof(AB)}");
        public static readonly MethodInfo CommadTetsMethod = typeof(CommandSetOne).GetMethod(nameof(CommadTets)) ?? throw new NullReferenceException($"Failed to find: {nameof(CommadTets)}");
        public static readonly MethodInfo smkdlmMethod = typeof(CommandSetOne).GetMethod(nameof(smkdlm)) ?? throw new NullReferenceException($"Failed to find: {nameof(smkdlm)}");
        public static readonly MethodInfo SMethod = typeof(CommandSetOne).GetMethod(nameof(S)) ?? throw new NullReferenceException($"Failed to find: {nameof(S)}");



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