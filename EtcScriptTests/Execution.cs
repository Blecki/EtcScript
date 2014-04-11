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
			TestHelper.RunSimpleTest("test foo {}");
        }

		[Test]
		public void simple_return()
		{
			TestHelper.RunSimpleTest(@"test foo 
	{return;}");
		}

		[Test]
		public void return_value()
		{
			TestHelper.RunSimpleTest(@"test foo
	{return 4 + 5;}", 9);
		}

		[Test]
		public void _if()
		{
			Assert.AreEqual(5,
				TestHelper.RunSimpleTest(@"test foo
	{let x = 4;
	if (x == 4)
{
		return 5;}
	else
		{return 9;}}
"));
		}

		[Test]
		public void _else_if()
		{
			Assert.AreEqual(9,
				TestHelper.RunSimpleTest(@"
test foo {
	let x = 7;
	if (x == 4) {
		return 5;
	} else if (x == 7) {
		return 9;
	} else {
		return 10;
	}
}
"));
		}

		[Test]
		public void local_variable()
		{
			Assert.AreEqual(9,
				TestHelper.RunSimpleTest(@"
test _ {
	var x = 3;
	return x * x;
}"));

			Assert.AreEqual(4,
				TestHelper.RunSimpleTest(@"
test _ {
	var x = 3;
	let x = 2;
	return x * x;
}"));
		}
    }

}