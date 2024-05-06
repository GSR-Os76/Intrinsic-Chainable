using GSR.CommandRunner;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestVariable
    {
        [TestMethod]
        [DataRow("d", null, null)]
        [DataRow("asSsOpe", typeof(int), 0)]
        public void TestValidConstruct(string name, Type? type, object? value) 
        {
            Variable v = new(name, type, value);
            Assert.AreEqual(v.Name, name);
            Assert.AreEqual(v.Type, type);
            Assert.AreEqual(v.Value, value);
        } // end TestValidConstruct()

        [TestMethod]
        public void TestSubtypeConstruct() 
        {
            string name = "_9";
            Type? type = typeof(Exception);
            object? value = new InvalidSyntaxException();
        
            Variable v = new(name, type, value);
            Assert.AreEqual(v.Name, name);
            Assert.AreEqual(v.Type, type);
            Assert.AreEqual(v.Value, value);
         }

        [TestMethod]
        [ExpectedException(typeof(TypeMismatchException))]
        [DataRow("d", typeof(string), null)]
        [DataRow("903kf", null, "")]
        [DataRow("4", typeof(int), 0f)]
        public void TestTypeMismatchConstruct(string name, Type? type, object? value) => new Variable(name, type, value);

        [TestMethod]
        [ExpectedException(typeof(TypeMismatchException))]
        public void TestSupertypeConstruct()
        {
            string name = "=";
            Type? type = typeof(InvalidSyntaxException);
            object? value = new Exception();

            new Variable(name, type, value);
        } // end TestSupertypeConstruct()

        // repeat ^ except with Update..()

    } // end class
} // end namespace