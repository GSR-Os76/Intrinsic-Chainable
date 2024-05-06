using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommandInterpreter
    {
        public CommandInterpreter Interpreter() => new CommandInterpreter(new CommandSet());

        // test string literal
        [TestMethod]
        [DataRow("\"st\"", "st")]
        [DataRow("\"\"", "")]
        [DataRow("\"\\\\\"", "\\")]
        [DataRow("\"\\\"\"", "\"")]
        [DataRow("\"\\\"hey\\\"\"", "\"hey\"")]
        [DataRow("\"943.0()$$\"", "943.0()$$")]
        public void TestStringLiteralInterpret(string command, string result) 
        {
            Assert.AreEqual(result, Interpreter().Evaluate(command).Execute());       
        } // end TestStringLiteralInterpret()

    } // end class
} // end namespace