using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class Rules
	{
		[Test]
		public void rules_compile()
		{
			var script = @"
rule foo (a) (b) bar {
	return stop;
}

test _ : number {
	return 6;	
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(6, result);
		}

		[Test]
		public void consider_rule()
		{
			var script = @"
rule foo (a) (b) bar {
	return continue;
}

test _ {
	consider [foo 1 2 bar];
}
";

			var result = TestHelper.CallTestFunction(script);
		}

		[Test]
		public void when()
		{
			var script = @"
rule foo (a) when false {
	fail;
}

rule foo (a) when true {
	return continue;
}

test _ {
	consider [foo 1];
}
";

			var result = TestHelper.CallTestFunction(script);

		}

		[Test]
		public void rule_types()
		{
			var script = @"
rule foo : string {
	return ""a"";
}

default of rule foo : string { return ""error""; }

test _ : string {
	return [consider [foo]];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.IsTrue(result.ToString() == "a");
		}

		[Test]
		public void rule_defaults()
		{
			var script = @"
rule foo : string when false {
	return ""a"";
}

default of rule foo : string {
	return ""b"";
}

test _ : string {
	return [consider [foo]];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.IsTrue(result.ToString() == "b");
		}

		[Test]
		public void rule_nevermind()
		{
			var script = @"
rule foo : string {
	return nevermind;
}

default of rule foo : string {
	return ""b"";
}

test _ : string {
	return [consider [foo]];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.IsTrue(result.ToString() == "b");
		}

		[Test]
		public void rule_order()
		{
			var script = @"
type A {}
type B : A {}

rule foo (a:A) : string {
	return ""a"";
}

rule foo (b:B) : String {
	return ""b"";
}

default of rule foo (a) : string {
	return ""c"";
}

test _ : string {
	var x = new B {};
	var y = new A {};
	return [consider [foo x]] + [consider [foo y]];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.IsTrue(result.ToString() == "ba");
		}

		[Test]
		public void rule_order_operator()
		{
			var script = @"
type A {}
type B : A {}

rule foo (a:A) : string {
	return ""a"";
}

# This rule should be ordered before the other, order last should override it.
rule foo (b:B) : String order last {
	return ""b"";
}

default of rule foo (a) : string { return ""ERROR""; }

test _ : string {
	var x = new B {};
	return [consider [foo x]];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.IsTrue(result.ToString() == "a");
		}
	}

}