using GSR.CommandRunner;
using System.Linq.Expressions;
using System.Reflection;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestMethodInfoCommand
    {
        public static readonly MethodInfo CommadTetsMethod = typeof(TestMethodInfoCommand).GetMethod(nameof(CommadTets)) ?? throw new NullReferenceException("Failed to find a method");



        [Command]
        public static void CommadTets(int a, string b) { }



        [TestMethod]
        public void TestCode() 
        {
            ICommand c = new MethodInfoCommand(CommadTetsMethod);
            Assert.AreEqual(c.Code, $"{nameof(CommadTets)}(_, _)");
        } // end TestCode


    } // end class
} // end namespace