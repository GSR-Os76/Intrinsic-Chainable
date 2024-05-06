using GSR.CommandRunner;

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
        [DataRow("\"k\\\\\"", "k\\")]
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

        [TestMethod]
        [DataRow("$Op=\"a\"")]
        [DataRow("$Pi=\"\\\\3.14\"")]
        public void TestAssignStringLiteralToVariable(string command)
        {
            Interpreter().Evaluate(command).Execute();
        } // end TestAssignVariable()

        [TestMethod]
        [DataRow("$Op=>\"a\"")]
        [DataRow("$Pi=>\"\\\\3.14\"")]
        public void TestFunctionAssignStringLiteralToVariable(string command)
        {
            Interpreter().Evaluate(command).Execute();
        } // end TestFunctionAssignVariable()

        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("$a=\"\"", "$a$")]
        [DataRow("$Ldk=\"val\ts\"", "$Ldk-")]
        [DataRow("$_o=\"0\"", "$_o\\")]
        public void TestVariableThenInvalid(string assignment, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assignment).Execute();
            ci.Evaluate(command); 
        } // end TestVariableThenInvalid()



        [TestMethod]
        [DataRow("$Var1=\"\\\"\"", "$Var1", "\"")]
        public void TestRetrieveAssignedStringLiteral(string assignment, string command, string result) 
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assignment).Execute();
            Assert.AreEqual(result, ci.Evaluate(command).Execute());
        } // end TestRetrieveAssignedStringLiteral

        [TestMethod]
        [DataRow("$Var1=>\"\\\"\"", "$Var1", "\"")]
        public void TestRetrieveFunctionAssignedStringLiteral(string assignment, string command, string result)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assignment).Execute();

            object? retrieved = ci.Evaluate(command).Execute();
            Assert.IsNotNull(retrieved);
            Assert.IsTrue(typeof(ICommand).IsAssignableFrom(retrieved.GetType()));
            Assert.AreEqual(result, ((ICommand)retrieved).Execute());
        } // end TestRetrieveAssignedStringLiteral



        [TestMethod]
        [DataRow("$Overwritable", "=", "\"a\"", "\"b\"", "b")]
        [DataRow("$a", "=>", "\"a\"", "\"\"", "")]
        [DataRow("$_UUID", "=>", "\"a\"", "\"b\"", "b")]
        [DataRow("$snake_cased", "=", "\"\"", "\"\\\\hello\\\\\"", @"\hello\")]
        public void TestSomeAssignThenAssignOverwrite(string varName, string initalAssignOperator, string initial, string then, string expected) 
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate($"{varName}{initalAssignOperator}{initial}").Execute();
            ci.Evaluate($"{varName}={then}").Execute();
            Assert.AreEqual(expected, ci.Evaluate(varName).Execute());
        } // end TestSomeAssignThenAssignOverwrite()

        [TestMethod]
        [DataRow("$Overwritable", "=", "\"a\"", "\"b\"", "b")]
        [DataRow("$a", "=>", "\"a\"", "\"\"", "")]
        [DataRow("$_UUID", "=>", "\"a\"", "\"b\"", "b")]
        [DataRow("$snake_cased", "=", "\"\"", "\"\\\\hello\\\\\"", @"\hello\")]
        public void TestSomeAssignThenFunctionAssignOverwrite(string varName, string initalAssignOperator, string initial, string then, string expected)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate($"{varName}{initalAssignOperator}{initial}").Execute();
            ci.Evaluate($"{varName}=>{then}").Execute();
            object? e1 = ci.Evaluate(varName).Execute();
            Assert.IsNotNull(e1);
            Assert.AreEqual(expected, ((ICommand)e1).Execute());
        } // end TestSomeAssignThenFunctionAssignOverwrite()

    } // end class
} // end namespace