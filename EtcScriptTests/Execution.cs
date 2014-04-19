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
			TestHelper.RunSimpleTest(@"test foo:number
	{return 4 + 5;}", 9);
		}

		[Test]
		public void _if()
		{
			TestHelper.RunSimpleTest(@"test foo:number
	{var x = 4;
	if (x == 4)
{
		return 5;}
	else
		{return 9;}}
", 5);
		}

		[Test]
		public void _else_if()
		{
			TestHelper.RunSimpleTest(@"
test foo:number {
	var x = 7;
	if (x == 4) {
		return 5;
	} else if (x == 7) {
		return 9;
	} else {
		return 10;
	}
}
", 9);
		}

		[Test]
		public void local_variable()
		{
			TestHelper.RunSimpleTest(@"
test _:number {
	var x = 3;
	return x * x;
}", 9);

			TestHelper.RunSimpleTest(@"
test _:number {
	var x = 3;
	let x = 2;
	return x * x;
}", 4);
		}

		[Test]
		public void system_variable()
		{
			var r = TestHelper.CallTestFunction("test _:number { return foo; }",
				(e) => { e.AddSystemVariable("foo", "NUMBER", (c) => { return 42; }); });

			Assert.AreEqual(42, r);
		}

		[Test]
		public void static_variable()
		{
			TestHelper.RunSimpleTest("global foo; test _ : generic { let foo = 5; return foo * 2; }", 10);
		}

		[Test]
		public void locals_hide_statics()
		{
			TestHelper.RunSimpleTest("global foo; test _ : generic { var foo = 2; return foo * foo; }", 4);
		}
    }

}