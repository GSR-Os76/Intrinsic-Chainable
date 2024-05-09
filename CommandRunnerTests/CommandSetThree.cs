using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    public class CommandSetThree
    {
        [Command]
        public int Add(int a, int b) => a + b;

        [Command]
        public string ToLower(string s) => s.ToLower();

        [Command]
        public string ToUpper(string s) => s.ToUpper();

        public string Range(string s, int start, int end) => s[(start< 0 ? ^start : start)..(end < 0 ? ^end : end)];

        public void Parameterless() { } // end Parameterless()

        public bool IsCommand(object? val) => typeof(ICommand).IsAssignableFrom(val?.GetType());

    } // end class
} // end namespace