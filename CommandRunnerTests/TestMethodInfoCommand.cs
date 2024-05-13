using GSR.CommandRunner;
using System.Text;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestMethodInfoCommand
    {
        [TestMethod]
        public void TestName1()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            Assert.AreEqual(c.Name, nameof(CommandSetOne.CommadTets));
        } // end TestCodeMultipleParameters()

        [TestMethod]
        public void TestName2()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.smkdlmMethod);
            Assert.AreEqual(c.Name, nameof(CommandSetOne.smkdlm));
        } // end TestCodeSingleParameters()



        [TestMethod]
        public void TestVoidReturnType()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.ABMethod);
            Assert.AreEqual(c.ReturnType, typeof(void));
        } // end TestVoidReturnType()

        [TestMethod]
        public void TestPrimitiveReturnType()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.smkdlmMethod);
            Assert.AreEqual(c.ReturnType, typeof(int));
        } // end TestPrimitiveReturnType()

        [TestMethod]
        public void TestReturnType()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            Assert.AreEqual(c.ReturnType, typeof(StringBuilder));
        } // end TestReturnType()



        [TestMethod]
        public void TestNoParameterTypes()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.ABMethod);
            Assert.AreEqual(c.ParameterTypes.Length, 0);
        } // end TestNoParameterTypes()

        [TestMethod]
        public void TestParameterTypes()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            Type[] expectation = new Type[] { typeof(int), typeof(string) };
            Assert.AreEqual(c.ParameterTypes.Length, expectation.Length);
            for (int i = 0; i < c.ParameterTypes.Length; i++)
                Assert.AreEqual(expectation[i], c.ParameterTypes[i]);
        } // end TestNoParameterTypes()



        [TestMethod]
        public void TestParameterlessResultlessExecution()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.ABMethod);
            object? r = c.Execute(new object[] { });
            Assert.IsNull(r);
        } // end TestParameterlessResultlessExecution()

        [TestMethod]
        [DataRow(12, "Some string something", "Some string something 12")]
        public void TestExecution(int a, string b, string result)
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            object? r = c.Execute(new object[] { a, b });
            Assert.AreEqual(r?.GetType(), typeof(StringBuilder));
            Assert.AreEqual(r?.ToString(), result);
        } // end TestExecution()

        [TestMethod]
        public void TestSubtypeArg()
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.SMethod);
            object? r = c.Execute(new object[] { new InvalidOperationException() });
            Assert.AreEqual(r, 0);
        } // end TestSubtypeArg()

        [TestMethod]
        [ExpectedException(typeof(InvalidCommandOperationException))]
        [DataRow(0)]
        [DataRow()]
        [DataRow(0, "", 909f)]
        [DataRow(0, 0, 0, 0)]
        public void TestExecuteWrongParameterCount(params object[] parameters)
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            c.Execute(parameters);
        } // end TestExecuteWrongParameterCount()

        [TestMethod]
        [ExpectedException(typeof(TypeMismatchException))]
        [DataRow(56.3f, "drftgyh uju66y_")]
        [DataRow(12f, .93d)]
        [DataRow("", 093)]
        public void TestExecuteWrongParameterTypes(params object[] parameters)
        {
            ICommand c = new MethodInfoCommand(CommandSetOne.CommadTetsMethod);
            c.Execute(parameters);
        } // end TestExecuteWrongParameterTypes()

    } // end class
} // end namespace