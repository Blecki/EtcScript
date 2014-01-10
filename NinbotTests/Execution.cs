using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NinbotTests
{
    [TestFixture]
    public class Execution
    {
        [Test]
        public void executes_trivial_script()
        {
			TestHelper.RunSimpleTest("Activity foo");
        }

    }

}