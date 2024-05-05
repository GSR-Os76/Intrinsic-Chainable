using GSR.CommandRunner;
using System.Text;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommand
    {
        public ICommand A() => new Command("A", typeof(int), new Type[] { typeof(int), typeof(int), typeof(int) }, (x) => (int)x[0] + ((int)x[1] - (int)x[2]));

        public ICommand B() => new Command("B", typeof(int), new Type[] { }, (x) => "" + 1+13);

        public ICommand SubCable() => new Command("SubCable", typeof(void), new Type[] { typeof(Exception) }, (x) => null);



        [TestMethod]
        public void TestCodeNoParameters()
        {
            Assert.AreEqual(B().Code, $"{B().Name}()");
        } // end TestCodeNoParameters()

        [TestMethod]
        public void TestCodeMultipleParameters()
        {
            Assert.AreEqual(A().Code, $"{A().Name}(?, ?, ?)");
        } // end TestCodeMultipleParameters()




        [TestMethod]
        public void TestParameterlessExecution()
        {
            object? r = B().Execute(new object[] { });
            Assert.AreEqual(r?.GetType(), typeof(string));
        } // end TestParameterlessExecution()

        [TestMethod]
        [DataRow(12, 9, 12, 9)]
        public void TestExecution(int a, int b, int c, int result)
        {
            object? r = A().Execute(new object[] { a, b, c });
            Assert.AreEqual(r?.GetType(), typeof(int));
            Assert.AreEqual(r, result);
        } // end TestExecution()

        [TestMethod]
        public void TestSubtypeArg()
        {
            object? r = SubCable().Execute(new object[] { new InvalidOperationException() });
            Assert.IsNull(r);
        } // end TestSubtypeArg()

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(0)]
        [DataRow()]
        [DataRow(0, "", 909f)]
        [DataRow(0, 0, 0, 0)]
        public void TestExecuteWrongParameterCount(params object[] parameters)
        {
            A().Execute(parameters);
        } // end TestExecuteWrongParameterCount()

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(56.3f, 90, 23)]
        [DataRow(12f, .93d, 2)]
        [DataRow("", 093, "arg")]
        public void TestExecuteWrongParameterTypes(params object[] parameters)
        {
            A().Execute(parameters);
        } // end TestExecuteWrongParameterTypes()

    } // end class
} // end namespace