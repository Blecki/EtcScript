using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class Macros
    {
		[Test]
		public void macros_callable()
		{
			var script = @"
macro foo (a) (b) bar {
	return 6;
}

test _ {
	return [foo 1 2 bar];
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(6, result);

		}

		[Test]
		public void correct_macro_chosen()
		{
			var script = @"
macro foo (a) (b) bar {
	return a;
}

macro foo (a) bar (b) {
	return b;
}

test _ {
	return [FOO 1 BAR 2];
}

";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(2, result);

		}

		[Test]
		public void macros_call_self()
		{
			var script = @"
macro fib (a) {
	if (a < 2) {
		return a;
	}
	return [fib (a - 1)] + [fib (a - 2)];
}

test _ {
	return [fib 6];
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(8, result);

		}      
    }

}