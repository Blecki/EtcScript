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
macro foo (a) (b) bar : generic {
	return 6;
}

test _ : generic {
	return [foo 1 2 bar];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(6, result);

		}

		[Test]
		public void correct_macro_chosen()
		{
			var script = @"
macro foo (a) (b) bar : generic {
	return a;
}

macro foo (a) bar (b) : generic {
	return b;
}

test _ : generic {
	return [FOO 1 BAR 2];
}

";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(2, result);

		}

		[Test]
		public void types_affect_macro_choice()
		{
			var script = @"
macro foo (a:number) : generic {
	return 2;
}

macro foo (a:string) : generic {
	return 4;
}

test _ : generic {
	return [FOO ""string!""];
}

";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(4, result);

		}

		[Test]
		public void macros_call_self()
		{
			var script = @"
macro fib (a:number) : number {
	if (a < 2) {
		return a;
	}
	return ([fib (a - 1)] + [fib (a - 2)]):number;
}

test _ : number {
	return [fib 6];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(8, result);

		}      
    }

}