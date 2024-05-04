using GSR.CommandRunner;
using System.Reflection;
using System.Text;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestMethodInfoCommand
    {
        public static readonly MethodInfo ABMethod = typeof(TestMethodInfoCommand).GetMethod(nameof(AB)) ?? throw new NullReferenceException($"Failed to find: {nameof(AB)}");

        public static readonly MethodInfo CommadTetsMethod = typeof(TestMethodInfoCommand).GetMethod(nameof(CommadTets)) ?? throw new NullReferenceException($"Failed to find: {nameof(CommadTets)}");

        public static readonly MethodInfo smkdlmMethod = typeof(TestMethodInfoCommand).GetMethod(nameof(smkdlm)) ?? throw new NullReferenceException($"Failed to find: {nameof(smkdlm)}");

        public static readonly MethodInfo SMethod = typeof(TestMethodInfoCommand).GetMethod(nameof(S)) ?? throw new NullReferenceException($"Failed to find: {nameof(S)}");



        [Command]
        public static void AB() { }

        [Command]
        public static StringBuilder CommadTets(int a, string b) => new StringBuilder(b).Append(" ").Append(a);

        [Command]
        public static int smkdlm(int a) => a;

        [Command]
        public static int S(Exception a) => 0;



        [TestMethod]
        public void TestCodeNoParameters()
        {
            ICommand c = new MethodInfoCommand(ABMethod);
            Assert.AreEqual(c.Code, $"{nameof(AB)}()");
        } // end TestCodeNoParameters()

        [TestMethod]
        public void TestCodeMultipleParameters()
        {
            ICommand c = new MethodInfoCommand(CommadTetsMethod);
            Assert.AreEqual(c.Code, $"{nameof(CommadTets)}(_, _)");
        } // end TestCodeMultipleParameters()

        [TestMethod]
        public void TestCodeSingleParameters()
        {
            ICommand c = new MethodInfoCommand(smkdlmMethod);
            Assert.AreEqual(c.Code, $"{nameof(smkdlm)}(_)");
        } // end TestCodeSingleParameters()



        [TestMethod]
        public void TestVoidReturnType()
        {
            ICommand c = new MethodInfoCommand(ABMethod);
            Assert.AreEqual(c.ReturnType, typeof(void));
        } // end TestVoidReturnType()

        [TestMethod]
        public void TestPrimitiveReturnType()
        {
            ICommand c = new MethodInfoCommand(smkdlmMethod);
            Assert.AreEqual(c.ReturnType, typeof(int));
        } // end TestPrimitiveReturnType()

        [TestMethod]
        public void TestReturnType()
        {
            ICommand c = new MethodInfoCommand(CommadTetsMethod);
            Assert.AreEqual(c.ReturnType, typeof(StringBuilder));
        } // end TestReturnType()



        [TestMethod]
        public void TestNoParameterTypes() 
        {
            ICommand c = new MethodInfoCommand(ABMethod);
            Assert.AreEqual(c.Parameters.Length, 0);
        } // end TestNoParameterTypes()

        [TestMethod]
        public void TestParameterTypes()
        {
            ICommand c = new MethodInfoCommand(CommadTetsMethod);
            Type[] expectation = new Type[] {typeof(int), typeof(string)};
            Assert.AreEqual(c.Parameters.Length, expectation.Length);
            for (int i = 0; i < c.Parameters.Length; i++) 
                Assert.AreEqual(expectation[i], c.Parameters[i]);
        } // end TestNoParameterTypes()



        [TestMethod]
        public void TestEmptyResultlessExecution() 
        {
            ICommand c = new MethodInfoCommand(ABMethod);
            object? r = c.Execute(new object[] { });
            Assert.IsNull(r);
        } // end TestEmptyResultlessExecution()

        [TestMethod]
        [DataRow(12, "Some string something", "Some string something 12")]
        public void TestExecution(int a, string b, string result)
        {
            ICommand c = new MethodInfoCommand(CommadTetsMethod);
            object? r = c.Execute(new object[] {a, b});
            Assert.AreEqual(r.GetType(), typeof(StringBuilder));
            Assert.AreEqual(r.ToString(), result);
        } // end TestEmptyResultlessExecution()

        [TestMethod]
        public void TestSubtypeArg()
        {
            ICommand c = new MethodInfoCommand(SMethod);
            object? r = c.Execute(new object[] { new InvalidOperationException() });
            Assert.AreEqual(r, 0);
        } // end TestSubtypeArg()

        } // end class
} // end namespace