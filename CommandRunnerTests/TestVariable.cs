using GSR.IntrinsicChainable;

namespace GSR.Tests.CommIntrinsicChainableandRunner
{
    [TestClass]
    public class TestVariable
    {
        [TestMethod]
        [DataRow("d", null)]
        [DataRow("asSsOpe", 0)]
        public void TestConstruct(string name, object? value)
        {
            Variable v = new(name, value);
            Assert.AreEqual(v.Name, name);
            Assert.AreEqual(v.Value, value);
        } // end TestConstruct()

    } // end class
} // end namespace