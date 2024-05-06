using GSR.CommandRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommandInterpreter
    {
        public CommandInterpreter Interpreter() => new CommandInterpreter(new CommandSet());



        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("")]
        [DataRow("  \n")]
        [DataRow("\t")]
        public void TestEmptyCommand(string command) => Interpreter().Evaluate(command);


        // test string literal
        [TestMethod]
        [DataRow("\"st\"", "st")]
        [DataRow("\"\"", "")]
        [DataRow("\"\\\\\"", "\\")]
        [DataRow("\"\\\"\"", "\"")]
        [DataRow("\"\\\"hey\\\"\"", "\"hey\"")]
        [DataRow("\"943.0()$$\"", "943.0()$$")]
        public void TestStringLiteralInterpret(string command, string result) => Assert.AreEqual(result, Interpreter().Evaluate(command).Execute());       

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestChainToStringLiteral() => Interpreter().Evaluate("\"a\".\"chainedTo\"");

        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("\"")]
        [DataRow("\"\\\"")]
        [DataRow("\"dsknjefk\\f")]
        public void TestUnclosedStringLiteral(string command) => Interpreter().Evaluate(command);



        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("$a")]
        [DataRow("$Ldk")]
        [DataRow("$_o")]
        public void TestUndeclaredVariableCall(string command) => Interpreter().Evaluate(command);


    } // end class
} // end namespace