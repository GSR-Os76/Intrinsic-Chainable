using GSR.IntrinsicChainable;

namespace GSR.Tests.IntrinsicChainable
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

        [TestMethod]
        public void TestGetCommand()
        {
            ICommandSet cs = new CommandSet(new List<Type>() { typeof(CommandSetOne), typeof(CommandSetTwo) });
            Assert.AreNotEqual(cs.GetCommand("AB", 0), cs.GetCommand("AB", 1));
            Assert.AreSame(cs.GetCommand("AB", 0), cs.GetCommand("AB", 0));
            Assert.AreSame(cs.GetCommand("AB", 1), cs.GetCommand("AB", 1));
        } // end TestGetCommand()

        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("", 0)]
        [DataRow("_Ps", 0)]
        [DataRow("AB", 3)]
        public void TestGetUnknownCommand(string name, int paramCount)
        {
            ICommandSet cs = new CommandSet(new List<Type>() { typeof(CommandSetOne), typeof(CommandSetTwo) });
            cs.GetCommand(name, paramCount);
        } // end TestGetUnknownCommand()

    } // end class
} // end namespace