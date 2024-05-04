using GSR.CommandRunner;
using System.Reflection;
using System.Text;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestMethodInfoCommand
    {
       
        [TestMethod]
        public void TestCodeNoParameters()
        {
            ICommand c = new MethodInfoCommand(Commands.ABMethod);
            Assert.AreEqual(c.Code, $"{nameof(Commands.AB)}()");
        } // end TestCodeNoParameters()

        [TestMethod]
        public void TestCodeMultipleParameters()
        {
            ICommand c = new MethodInfoCommand(Commands.CommadTetsMethod);
            Assert.AreEqual(c.Code, $"{nameof(Commands.CommadTets)}(?, ?)");
        } // end TestCodeMultipleParameters()

        [TestMethod]
        public void TestCodeSingleParameters()
        {
            ICommand c = new MethodInfoCommand(Commands.smkdlmMethod);
            Assert.AreEqual(c.Code, $"{nameof(Commands.smkdlm)}(?)");
        } // end TestCodeSingleParameters()



        [TestMethod]
        public void TestVoidReturnType()
        {
            ICommand c = new MethodInfoCommand(Commands.ABMethod);
            Assert.AreEqual(c.ReturnType, typeof(void));
        } // end TestVoidReturnType()

        [TestMethod]
        public void TestPrimitiveReturnType()
        {
            ICommand c = new MethodInfoCommand(Commands.smkdlmMethod);
            Assert.AreEqual(c.ReturnType, typeof(int));
        } // end TestPrimitiveReturnType()

        [TestMethod]
        public void TestReturnType()
        {
            ICommand c = new MethodInfoCommand(Commands.CommadTetsMethod);
            Assert.AreEqual(c.ReturnType, typeof(StringBuilder));
        } // end TestReturnType()



        [TestMethod]
        public void TestNoParameterTypes() 
        {
            ICommand c = new MethodInfoCommand(Commands.ABMethod);
            Assert.AreEqual(c.Parameters.Length, 0);
        } // end TestNoParameterTypes()

        [TestMethod]
        public void TestParameterTypes()
        {
            ICommand c = new MethodInfoCommand(Commands.CommadTetsMethod);
            Type[] expectation = new Type[] {typeof(int), typeof(string)};
            Assert.AreEqual(c.Parameters.Length, expectation.Length);
            for (int i = 0; i < c.Parameters.Length; i++) 
                Assert.AreEqual(expectation[i], c.Parameters[i]);
        } // end TestNoParameterTypes()



        [TestMethod]
        public void TestEmptyResultlessExecution() 
        {
            ICommand c = new MethodInfoCommand(Commands.ABMethod);
            object? r = c.Execute(new object[] { });
            Assert.IsNull(r);
        } // end TestEmptyResultlessExecution()

        [TestMethod]
        [DataRow(12, "Some string something", "Some string something 12")]
        public void TestExecution(int a, string b, string result)
        {
            ICommand c = new MethodInfoCommand(Commands.CommadTetsMethod);
            object? r = c.Execute(new object[] {a, b});
            Assert.AreEqual(r.GetType(), typeof(StringBuilder));
            Assert.AreEqual(r.ToString(), result);
        } // end TestEmptyResultlessExecution()

        [TestMethod]
        public void TestSubtypeArg()
        {
            ICommand c = new MethodInfoCommand(Commands.SMethod);
            object? r = c.Execute(new object[] { new InvalidOperationException() });
            Assert.AreEqual(r, 0);
        } // end TestSubtypeArg()

        } // end class
} // end namespace