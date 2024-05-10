using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    public static class CommandSetThree
    {
        [Command]
        public static int Add(int a, int b) => a + b;

        [Command]
        public static string ToLower(string s) => s.ToLower();

        [Command]
        public static string ToUpper(string s) => s.ToUpper();

        [Command]
        public static string Range(string s, int start, int end) => s[(start< 0 ? ^start : start)..(end < 0 ? ^end : end)];

        [Command]
        public static int Count(string s) => s.Length;

        [Command]
        public static void Parameterless() { } // end Parameterless()

        [Command]
        public static bool IsCommand(object? val) => typeof(ICommand).IsAssignableFrom(val?.GetType());

    } // end class
} // end namespace