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
	return 6;
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
	return 1;
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
	return 1;
}

test _ {
	consider [foo 1];
}
";

			var result = TestHelper.CallTestFunction(script);

		}
	}

}