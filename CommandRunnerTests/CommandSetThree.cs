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
        public static string Range(string s, int start, int end) => s[(start< 0 ? ^Math.Abs(start) : start)..(end < 0 ? ^Math.Abs(end) : end)];

        [Command]
        public static int Count(string s) => s.Length;

        [Command]
        public static void Parameterless() { } // end Parameterless()

        [Command]
        public static bool IsCommand(object? val) => typeof(ICommand).IsAssignableFrom(val?.GetType());

        [Command]
        public static bool HasXParameters(ICommand c, int x) => c.ParameterTypes.Length == x;

        [Command]
        public static ICommand ReturnsCommand() => new Command("_p", typeof(string), new Type[] { typeof(string), typeof(string) }, (x) => (string)(x[0] ?? throw new ArgumentNullException()) + (string)(x[1] ?? throw new ArgumentNullException()));

    } // end class
} // end namespace