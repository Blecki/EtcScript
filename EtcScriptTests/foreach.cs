using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class Foreach
	{
		[Test]
		public void foreach_in_loop()
		{
			Assert.AreEqual(4, TestHelper.CallTestFunction(@"
test _ : number {
	var y = 0;
	foreach x in [foo] {
		let y = y + 1;
	}
	return y;
}",
				e => e.AddSystemMacro("foo : LIST", (c, l) => new List<Object>(new Object[] { 0, 1, 2, 3 }))));
		}

		[Test]
		public void foreach_in_type_aliasing()
		{
			Assert.AreEqual("-abc", TestHelper.CallTestFunction(@"
type aliased-list {}
macro get at (n:number) from (l:aliased-list) : string { return ((l):list@n):string; }
macro convert (l:aliased-list) to list : list { return (l):list; }
macro convert (l:list) to aliased-list : aliased-list { return (l):aliased-list; }
macro add (a:string) (b:string) : string { return a + b; } #Just to check types

global f:aliased-list = { ""a"" ""b"" ""c"" };

test _ : string {
	var y = ""-"";
	foreach x in f {
		let y = [add y x];
	}
	return y;
}"));
		}

		[Test]
		public void foreach_from_loop()
		{
			Assert.AreEqual(9, TestHelper.CallTestFunction(@"
test _ : number {
	var t = 0;
	foreach x from 0 to 8 {
		let t = t + 1;
	}
	return t;
}"));
		}

		[Test]
		public void while_loop()
		{
			Assert.AreEqual(5, TestHelper.CallTestFunction(@"
test _ : number {
	var t = 0;
	while (t < 5) {
		let t = t + 1;
	}
	return t;
}"));
		}

	}
}