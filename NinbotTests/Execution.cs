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

		[Test]
		public void parenthical_becomes_invokation()
		{
			TestHelper.RunSimpleTest(@"Activity foo
	bar [token.foo]
");
			TestHelper.RunSimpleTest(@"Activity foo
	bar token.foo
");
		}

		[Test]
		public void simple_return()
		{
			TestHelper.RunSimpleTest(@"Activity foo
	return");
		}

		[Test]
		public void return_value()
		{
			TestHelper.RunSimpleTest(@"Activity foo
	return 4 + 5", 9);
		}

    }

}