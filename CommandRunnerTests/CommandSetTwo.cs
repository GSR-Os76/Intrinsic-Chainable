using GSR.IntrinsicChainable;

namespace GSR.Tests.IntrinsicChainable
{
    public class CommandSetTwo
    {
        [Command]
        public static void AB(float f) { }

        [Command]
        public static void AbC(long log) { }
    } // end class
} // end namespace