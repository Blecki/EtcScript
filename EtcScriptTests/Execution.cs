using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
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

		[Test]
		public void _if()
		{
			Assert.AreEqual(5,
				TestHelper.RunSimpleTest(@"Activity foo
	let x = 4
	if x == 4
		return 5
	else
		return 9
"));
		}

		[Test]
		public void _else_if()
		{
			Assert.AreEqual(9,
				TestHelper.RunSimpleTest(@"Activity foo
	let x = 7
	if x == 4
		return 5
	else if x == 7
		return 9
	else
		return 10
"));
		}
    }

}