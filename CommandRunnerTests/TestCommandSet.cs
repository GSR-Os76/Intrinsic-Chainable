using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestCommandSet
    {


        [TestMethod]
        public void TestList() 
        {
            ICommandSet cs = new CommandSet(new List<Type> { typeof(Commands) });
            Assert.AreEqual(cs.Commands.Count, 4);
        } // end TestList()

    } // end class
} // end namespace