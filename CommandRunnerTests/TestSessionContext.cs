using GSR.CommandRunner;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace GSR.Tests.CommandRunner
{
    [TestClass]
    public class TestSessionContext
    {

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow("")]
        [DataRow("fjisoroeiwq ")]
        [DataRow("_e,l_ dlf")]
        [DataRow("ʡ")]
        public void TestGetValueNonExistant(string name) => new SessionContext().GetValue(name, typeof(object));

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        [DataRow("k", 90, typeof(float))]
        public void TestGetValueWrongType(string name, object value, Type wrongGuess) 
        {
            SessionContext sc = new();
            sc.SetValue(name, value);
            sc.GetValue(name, wrongGuess);
        } // end TestGetValueWrongType()

        [TestMethod]
        [DataRow("k", "", typeof(object))]
        public void TestGetValueSubType(string name, object value, Type guess)
        {
            SessionContext sc = new();
            sc.SetValue(name, value);

            Logger.LogMessage(guess.ToString());
            Logger.LogMessage(value.GetType().ToString());
            Logger.LogMessage(value.GetType().IsSubclassOf(guess).ToString());

            Assert.AreEqual(sc.GetValue(name, guess), value);
        } // end TestGetValueSubType()

        [TestMethod]
        [DataRow("k", "", typeof(object))]
        public void TestGetValueSameType(string name, object value, Type guess)
        {
            SessionContext sc = new();
            sc.SetValue(name, value);
            Assert.AreEqual(sc.GetValue(name, guess), value);
        } // end TestGetValueSameType()

    } // end class
} // end namespace