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

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow("$O=\"0O0\"", "$O.\"Whatever\"")]
        public void TestVariableChainedToStringLiteral(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestVariableChainedToStringLiteral()



        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow("$O=\"0O0\"", "$O.0")]
        [DataRow("$O=\"0O0\"", "$O.9")]
        [DataRow("$O=\"0O0\"", "$O.68")]
        public void TestVariableChainedToNumericLiteral(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestVariableChainedToNumericLiteral()



        [TestMethod]
        [DataRow("-32768s", short.MinValue)]
        [DataRow("-90s", (short)-90)]
        [DataRow("-0s", (short)0)]
        [DataRow("0s", (short)0)]
        [DataRow("56s", (short)56)]
        [DataRow($"32767s", short.MaxValue)]
        [DataRow("-2147483648i", int.MinValue)]
        [DataRow("-9i", -9)]
        [DataRow("-0i", 0)]
        [DataRow("0i", 0)]
        [DataRow("04i", 4)]
        [DataRow("2147483647i", int.MaxValue)]
        [DataRow("-9223372036854775808l", long.MinValue)]
        [DataRow("-932l", -932l)]
        [DataRow("-0l", 0l)]
        [DataRow("0l", 0l)]
        [DataRow("1024l", 1024l)]
        [DataRow("9223372036854775807l", long.MaxValue)]
        [DataRow("-9384736478f", -9384736478f)]
        [DataRow("-4239.4f", -4239.4f)]
        [DataRow("-0.001f", -.001f)]
        [DataRow("-0.00f", 0f)]
        [DataRow("-0f", 0f)]
        [DataRow("0f", 0f)]
        [DataRow("0.0f", 0f)]
        [DataRow("423f", 423f)]
        [DataRow("5.5f", 5.5f)]
        [DataRow("2.0f", 2f)]
        [DataRow("-9384736478d", -9384736478d)]
        [DataRow("-5003.4d", -5003.4d)]
        [DataRow("-0.001d", -.001d)]
        [DataRow("-0.00d", 0d)]
        [DataRow("-0d", 0d)]
        [DataRow("0d", 0d)]
        [DataRow("0.0d", 0d)]
        [DataRow("423d", 423d)]
        [DataRow("5.5d", 5.5d)]
        [DataRow("2.0d", 2d)]
        public void TestValidNumericLiterals(string command, object expectation)
        {
            ICommandInterpreter ci = Interpreter();
            object? r = ci.Evaluate(command).Execute();
            Assert.IsNotNull(r);
            Assert.AreEqual(expectation.GetType(), r.GetType());
            Assert.AreEqual(expectation, r);
        } // end TestValidNumericLiterals()

        [TestMethod]
        public void TestValidDecimalNumericLiterals()
        {
            (new Tuple<string, decimal>[]
            {
                 Tuple.Create ("-123456789.0m", -123456789m),
                Tuple.Create ("-1293021m", -1293021m),
                Tuple.Create ("-90.9m", -90.9m),
                Tuple.Create ("-0m", 0m),
                Tuple.Create ("0m", 0m),
                Tuple.Create ("0.0m", 0m),
                Tuple.Create ("423m", 423m),
                Tuple.Create ("5.5m", 5.5m),
                Tuple.Create ("2.0m", 2m)
            }).ToList().ForEach((x) =>
            {
                string command = x.Item1;
                object expectation = x.Item2;
                ICommandInterpreter ci = Interpreter();
                object? r = ci.Evaluate(command).Execute();
                Assert.IsNotNull(r);
                Assert.AreEqual(expectation.GetType(), r.GetType());
                Assert.AreEqual(expectation, r);
            });
        } // end TestValidNumericLiterals()

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        [DataRow("-32769s")]
        [DataRow($"32768s")]
        [DataRow("-2147483649i")]
        [DataRow("2147483648i")]
        [DataRow("-9223372036854775809l")]
        [DataRow("9223372036854775808l")]
        [DataRow("-79228162514264337593543950336m")]
        [DataRow("79228162514264337593543950336m")]
        public void TestOverflowNumericLiterals(string command) => Interpreter().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void TestOverflowFloatAndDecimalNumericLiteral() 
        {
            (new string[]
            {
                 $"-35{new string('0', 37)}f",
                 $"35{new string('0', 37)}f",
                 $"-18{new string('0', 307)}d",
                 $"18{new string('0', 307)}d",
            }).ToList().ForEach((x) => Interpreter().Evaluate(x));
        } // end TestOverflowFloatAndDecimalNumericLiteral


        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("1.7s")]
        [DataRow("1.0s")]
        [DataRow("-1.9s")]
        [DataRow("1.1i")]
        [DataRow("1.0i")]
        [DataRow("-1.0i")]
        [DataRow("134.7l")]
        [DataRow("8.0l")]
        [DataRow("-0.990l")]
        public void TestIntegralNumericLiteralDisallowsDecimals(string command) => Interpreter().Evaluate(command);

#warning test lots of bad syntax input. like "903-k42d" or "$-30flp 0"

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow("$Y=\"ll_o27-=d\"", "$Y()")]
        public void TestInvokeNonCommandVariable(string assign, string command) 
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestInvokeNonCommandVariable()

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow("$Y=>\"poiuytfdx nmkjhyt\"", "\"Some string value\".$Y()")]
        [DataRow("$Y=>290i", "\"Se\".$Y()")]
        [DataRow("$Y=>\"(_#KD(\"", "26.12056f.$Y()")]
        [DataRow("$Y=>0f", "23m.$Y()")]
        public void TestChainToParameterlessCommandVariable(string assign, string command) 
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // endTestChainToParameterlessCommandVariable

#warning test chain to command variable type mismatchs;

        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("$m_Sar=>\"\"", "$m_Sar()5")]
        [DataRow("$m_Sar=>\"\"", "$m_Sar();")]
        [DataRow("$m_Sar=>\"\"", "$m_Sar()m")]
        [DataRow("$m_Sar=>\"\"", "$m_Sar()^")]
        public void TestVariableInvokeThenInvalidCharacter(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestVariableInvokeThenInvalidCharacter()



    } // end class
} // end namespace