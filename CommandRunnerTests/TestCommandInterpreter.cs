using GSR.CommandRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommandInterpreter
    {
        const int MetaCommandCount = 3;

        public CommandInterpreter Interpreter() => new CommandInterpreter(new CommandSet());

        public ICommandInterpreter Interpreter2() => new CommandInterpreter(new CommandSet(typeof(CommandSetThree)));



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
        [ExpectedException(typeof(InvalidCommandOperationException))]
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
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow("$O=\"0O0\"", "$O.\"Whatever\"")]
        public void TestVariableChainedToStringLiteral(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestVariableChainedToStringLiteral()



        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
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
        [DataRow("-932l", -932L)]
        [DataRow("-0l", 0L)]
        [DataRow("0l", 0L)]
        [DataRow("1024l", 1024L)]
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
        [ExpectedException(typeof(NumericOverflowException))]
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
        [ExpectedException(typeof(NumericOverflowException))]
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

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow("$Y=\"ll_o27-=d\"", "$Y()")]
        public void TestInvokeNonCommandVariable(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestInvokeNonCommandVariable()

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
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

        [TestMethod]
        [DataRow("$lT9T=> \"7\"", "$lT9T()", "7")]
        [DataRow("$opj  => 256s", "$opj()", (short)256)]
        public void TestVariableInvoke(string assign, string command, object expectation)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            Assert.AreEqual(expectation, ci.Evaluate(command).Execute());
        } // end TestVariableInoke()



        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("$A => $B = \"    \"", "$B")]
        [DataRow("$A => $C0_ => \"AREWETHEREYET\"", "$C0_")]
        public void TestVariableAssignmentFunctionAssignmentUninvoked(string assign, string fails)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(fails);
        } // end TestVariableAssignmentFunctionAssignmentUninvoked()


        [TestMethod]
        [DataRow("$A => $B = 80d", "$A()", "$B", 80d)]
        public void TestVariableAssignmentFunctionAssignmentInvoked(string assign, string execute, string testCommand, object expected)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(execute).Execute();
            Assert.AreEqual(expected, ci.Evaluate(testCommand).Execute());
        } // end TestVariableAssignmentFunctionAssignmentInvoked()

        [TestMethod]
        [DataRow("$A => $B => 80d", "$A()", "$B", 80d)]
        public void TestVariableFunctionAssignmentFunctionAssignmentInvoked(string assign, string execute, string testCommand, object expected)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(execute).Execute();
            object? f = ci.Evaluate(testCommand).Execute();
            Assert.IsNotNull(f);
            Assert.AreEqual(expected, ((ICommand)f).Execute());
        } // end TestVariableFunctionAssignmentFunctionAssignmentInvoked()

        [TestMethod]
        [DataRow("$R = 0l", "$F => $R = \"\"", "$F()", "$R")]
        public void TestVariableReassignmentFunctionAssignmentInvoked(string assign1, string assign2, string execute2, string unwrapMaybeReassignedVariable)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign1).Execute();
            ci.Evaluate(assign2).Execute();
            object? before = ci.Evaluate(unwrapMaybeReassignedVariable).Execute();
            ci.Evaluate(execute2).Execute();
            Assert.AreNotEqual(before, ci.Evaluate(unwrapMaybeReassignedVariable).Execute());
        } // end TestVariableReassignmentFunctionAssignmentInvoked()



        [TestMethod]
        [DataRow("$ShouldBeNull = $NN = 2049.4f", "$ShouldBeNull", "$NN", 2049.4f)]
        public void TestVariableAssignmentAssignmentLeavesVariableNull(string assign, string nullVar, string notNullVar, object notNullVal)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign).Execute();
            Assert.IsNull(ci.Evaluate(nullVar).Execute());
            Assert.AreEqual(ci.Evaluate(notNullVar).Execute(), notNullVal);
        } // end TestVariableAssignmentAssignmentLeavesVariableNull()

        [TestMethod]
        [DataRow("$R = 0l", "$F = $R = \"\"", "$R")]
        public void TestVariableReassignmentAssignmentNeedsNoInvoke(string assign1, string assign2, string unwrapMaybeReassignedVariable)
        {
            ICommandInterpreter ci = Interpreter();
            ci.Evaluate(assign1).Execute();
            object? before = ci.Evaluate(unwrapMaybeReassignedVariable).Execute();
            ci.Evaluate(assign2).Execute();
            Assert.AreNotEqual(before, ci.Evaluate(unwrapMaybeReassignedVariable).Execute());
        } // end TestVariableReassignmentAssignmentNeedsNoInvoke()

        // test regular s null assign




        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("^")]
        [DataRow("^")]
        [DataRow("%")]
        [DataRow("&")]
        [DataRow("@#s")]
        [DataRow("+90")]
        public void TestInvalidSyntaxFromBeginning(string command) => Interpreter().Evaluate(command);



        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("~A(%")]
        [DataRow("~A(,,)")]
        [DataRow("~A(,)")]
        public void TestUnreadableArgList(string command) => Interpreter().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow("\"\".~Variables()")]
        [DataRow("0f.~Commands()")]
        [DataRow("\"Some Test In a String\".~notRealCommand()")]
        [DataRow("\"\".~notRealCommand()")]
        [DataRow("324893i.>~Help()")]
        [DataRow("9d.>  ~Commands()")]
        public void TestChainToMetaCommand(string command) => Interpreter().Evaluate(command);

        [TestMethod]
        [DataRow("~Variables()", 2)]
        [DataRow("~Variables()", 4, "$A=0i", "$B=\"l\"")]
        [DataRow("~Variables()", 3, "$HowAreYou=\"Fine\"", "$HowAreYou=\"Bad\"")]
        [DataRow("~Variables()", 4, "$Var1 = $Var2 = 0s")]
        [DataRow("~Variables()", 3, "$Var1 =>$Var2= 0s")]
        public void TestMetaCommandVariables(string command, int newlinesExpected, params string[] var)
        {
            ICommandInterpreter ci = Interpreter();
            var.ToList().ForEach((x) => ci.Evaluate(x).Execute());

            object? rawValue = ci.Evaluate(command).Execute();
            Assert.IsNotNull(rawValue);
            string value = (string)rawValue;
            Logger.LogMessage(value); // intentionally left for the time
            Assert.AreEqual(newlinesExpected, value.Count((x) => x == '\n'));
        } // end TestMetaCommandVariables

        [TestMethod]
        public void TestMetaCommandCommandsDefault()
        {
            ICommandInterpreter ci = Interpreter();

            object? rawValue = ci.Evaluate("~Commands()").Execute();
            Assert.IsNotNull(rawValue);
            string value = (string)rawValue;
            Logger.LogMessage(value); // intentionally left for the times
            Assert.AreEqual(2 + MetaCommandCount, value.Count((x) => x == '\n'));
        } // end TestMetaCommandCommands()

        [TestMethod]
        public void TestMetaCommandCommands()
        {
            ICommandInterpreter ci = new CommandInterpreter(new CommandSet(new List<Type>() { typeof(CommandSetOne), typeof(CommandSetTwo) }));

            object? rawValue = ci.Evaluate("~Commands()").Execute();
            Assert.IsNotNull(rawValue);
            string value = (string)rawValue;
            Logger.LogMessage(value); // intentionally left for the times
            Assert.AreEqual(2 + MetaCommandCount + 6, value.Count((x) => x == '\n'));
        } // end TestMetaCommandCommands()

        [TestMethod]
        public void TestMetaCommandHelp()
        {
            object? rawValue = Interpreter().Evaluate("~Help()").Execute();
            Assert.IsNotNull(rawValue);
            string value = (string)rawValue;
            Logger.LogMessage(value); // intentionally left for the times
        } // end TestMetaCommandHelp()



        [TestMethod]
        [DataRow("~Help().ToLower()")]
        public void TestMetaCommandChainToSingleParamCommand(string command)
        {
            object? v = Interpreter2().Evaluate(command).Execute();
            Assert.IsNotNull(v);
            Assert.IsTrue(((string)v).All((x) => !char.IsLetter(x) ^ char.IsLower(x)));
        } // end TestMetaCommandChainToSingleParamCommand()

        [TestMethod]
        [DataRow("90i.Add(1i)", 91)]
        public void TestChainWithAnotherParam(string command, object expectation)
        {
            object? v = Interpreter2().Evaluate(command).Execute();
            Assert.AreEqual(v, expectation);
        } // end TestChainWithAnotherParam()

        [TestMethod]
        [DataRow("ToLower(?).Range(0i, ? ) ", typeof(string), typeof(string), typeof(int))]
        [DataRow("ToUpper(?).Count()", typeof(int), typeof(string))]
        public void TestParameterization(string command, Type expectedReturnType, params Type[] expectedParamTypes)
        {
            ICommandInterpreter ci = Interpreter2();
            ICommand c = ci.Evaluate(command);

            Assert.AreEqual(expectedReturnType, c.ReturnType);
            Assert.AreEqual(expectedParamTypes.Length, c.ParameterTypes.Length);
            for (int i = 0; i < expectedParamTypes.Length; i++)
                Assert.AreEqual(expectedParamTypes[i], c.ParameterTypes[i]);
        } // end TestParameterization

        [TestMethod]
        [DataRow("$L => ToLower(?)", "\"AND sO We sEe\".$L()", "and so we see")]
        public void TestChainToVariableHoldingParameterized(string assign, string command, object expectation)
        {
            ICommandInterpreter ci = Interpreter2();
            ci.Evaluate(assign).Execute();
            object? v = ci.Evaluate(command).Execute();
            Assert.AreEqual(expectation, v);
        } // end TestChainToVariableHoldingParameterized()

        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("~Commands((")]
        [DataRow("Range(\"Y\", 0, -1(")]
        public void TestOpenParenthesisInsteadOfCloseInInvocation(string command) => Interpreter2().Evaluate(command);

        [TestMethod]
        [DataRow("\"\".>IsCommand()", true)]
        [DataRow("~Variables().>IsCommand()", true)]
        [DataRow("10.10f.>IsCommand()", true)]
        [DataRow("\"\".IsCommand()", false)]
        [DataRow("~Variables().IsCommand()", false)]
        [DataRow("10.10f.IsCommand()", false)]
        public void TestFunctionChaining(string command, object? expectation) => Assert.AreEqual(Interpreter2().Evaluate(command).Execute(), expectation);

        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("ToLower()")]
        [DataRow("\"fyujng34|\".Range(0)")]
        public void TestWrongArgumentCount(string command) => Interpreter2().Evaluate(command);


        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("7l.Parameterless()")]
        [DataRow("\"\".Parameterless()")]
        // variable?
        public void TestChainToParameterless(string command) => Interpreter2().Evaluate(command);

        [TestMethod]
        [DataRow("IsCommand(>\"\")", true)]
        [DataRow("IsCommand(>~Variables())", true)]
        [DataRow("IsCommand(>10.10f)", true)]
        [DataRow("IsCommand(\"\")", false)]
        [DataRow("IsCommand(~Variables())", false)]
        [DataRow("IsCommand(10.10f)", false)]
        public void TestFunctionArg(string command, object? expectation) => Assert.AreEqual(Interpreter2().Evaluate(command).Execute(), expectation);

        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("A()")]
        [DataRow("Variables()")]
        [DataRow("A324od_3kd()")]
        public void TestUndefinedCommand(string command) => Interpreter().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow("ToLower(>?)")]
        [DataRow("Range(?, 0f, >?)")]
        public void TestFunctionParameterization(string command) => Interpreter2().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow("$_4 =>ToUpper(?)", "$_4(>?)")]
        public void TestFunctionParameterizationOfVariable(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter2();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestFunctionParameterizationOfVariable()

        [TestMethod]
        [ExpectedException(typeof(InvalidSyntaxException))]
        [DataRow("$Arv = > \"\"")]
        public void TestInvalidFunctionAssign(string command) => Interpreter().Evaluate(command);

        [TestMethod]
        [DataRow("ToUpper(?)", "A", "a")]
        [DataRow("ToUpper(?)", "A", "A")]
        [DataRow("ToLower(ToUpper(?))", "postular", "PostUlAr")]
        [DataRow("Range(ToLower(ToUpper(?)), 0i, ?)", "po", "PostUlAr", 2)]
        public void TestInvokeParameterized(string command, object? expectation, params object?[] args)
        {
            ICommandInterpreter ci = Interpreter2();
            ICommand c = ci.Evaluate(command);
            Assert.AreEqual(expectation, c.Execute(args));
        } // end TestInvokeParameterized()

        [TestMethod]
        [DataRow("$K_ela => Count(?)", "$K_ela.Add(9i)", 18, "~NineLong")]
        // [DataRow("$K_ela => Count(?)", "Add($K_ela, 9i)", 18, "123456789")]
        [DataRow("$_9 => \"ARms123345\".Range(?, ?)", "$_9.Range(0i, ?)", "ARms1233", 0, -1, -1)]
        public void TestParameterizedInsideVariable(string varAssign, string command, object? expectation, params object?[] args)
        {
            ICommandInterpreter ci = Interpreter2();
            ci.Evaluate(varAssign).Execute();
            ICommand c = ci.Evaluate(command);
            Assert.AreEqual(expectation, c.Execute(args));
        } // end TestParameterizedInsideVariable()


        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("~help()")]
        [DataRow("~CommandS()")]
        [DataRow("range()")]
        [DataRow("COUNT()")]
        public void TestCaseSensitivity(string command) => Interpreter2().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("$A = 1d", "$a")]
        [DataRow("$opLe_ = 1d", "$oplE_")]
        public void TestVariableCaseSensitivity(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter2();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestVariableCaseSensitivity()

        [TestMethod]
        [DataRow("ReturnsCommand().HasXParameters(?)", 1)]
        [DataRow("HasXParameters(ReturnsCommand(), ?)", 1)]
        public void TestReturnsCommandChainedToCommand(string command, int paramCount) => Assert.AreEqual(paramCount, Interpreter2().Evaluate(command).ParameterTypes.Length);



        [TestMethod]
        [ExpectedException(typeof(TypeMismatchException))]
        [DataRow("0i.Count()")]
        [DataRow("\";;;;;;;;;;;;;;;;;;\".Count().Range(0, 0)")]
        public void TestChainCommandTypeMismatch(string command) => Interpreter2().Evaluate(command);

        [TestMethod]
        [ExpectedException(typeof(TypeMismatchException))]
        [DataRow("$OneG => Count(?)", "948l.$OneG()")]
        [DataRow("$Q => HasXParameters(?, 9i)", "\"akor\".$Q()")]
        public void TestChainCommandVariableTypeMismatch(string assign, string command)
        {
            ICommandInterpreter ci = Interpreter2();
            ci.Evaluate(assign).Execute();
            ci.Evaluate(command);
        } // end TestChainCommandVariableTypeMismatch()



        [TestMethod]
        [ExpectedException(typeof(InterpreterException), AllowDerivedTypes = true)]
        [DataRow("oplE_")]
        [DataRow("~")]
        [DataRow("$")]
        [DataRow("?")]
        [DataRow(">")]
        [DataRow(".~Help")]
        [DataRow("~Help")]
        [DataRow("$E = ~Help")]
        [DataRow("$E => ~Help")]
        [DataRow("$OP p")]
        [DataRow("$Var = 0i l")]
        [DataRow("0I")]
        [DataRow("0S")]
        [DataRow("0M")]
        [DataRow("0o")]
        [DataRow("0b")]
        [DataRow("")]
        [DataRow("$0_ = 0i")]
        [DataRow("$903-k42d")]
        [DataRow("$-30flp 0")]
        // more bad chains, and bad command'sings
        // actually  currently works, and is fine. No reason to limit freedom I see. [DataRow("$ = 0i")]
        [DataRow("~Commands(\"\")")]
        [DataRow("s(a,)")]
        [DataRow("s(a)")]
        [DataRow("asdfsd()")]
        [DataRow("s(a,")]
        [DataRow(",a()")]
        [DataRow(".a()")]
        [DataRow("ToD.()_")]
        public void ThrowsInterpreterException(string command) => Interpreter().Evaluate(command);

#warning assure always lazy. Including parameterized args, arguments that're chained, arguments that're functional, literals, etc.

    } // end class
} // end namespace