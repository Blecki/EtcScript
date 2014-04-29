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
	}

}