using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommandSet
    {
        [TestMethod]
        public void TestList() 
        {
            ICommandSet cs = new CommandSet(typeof(CommandSetOne));
            Assert.AreEqual(cs.Commands.Count, 4);
        } // end TestList()

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCollisionDetection()
        {
            new CommandSet(new List<Type>() { typeof(CommandSetOne), typeof(CommandSetOne) });
        } // end TestCollisionDetection()

        [TestMethod]
        public void TestCollisionDetectionOverloading()
        {
            ICommandSet cs = new CommandSet(new List<Type>() { typeof(CommandSetOne), typeof(CommandSetTwo) });
            Assert.AreEqual(cs.Commands.Count, 6);
        } // end TestCollisionDetection()
    } // end class
} // end namespace