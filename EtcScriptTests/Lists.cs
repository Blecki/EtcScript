using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class Lists
	{
		[Test]
		public void list_index_get()
		{
			var script = @"
test _ : number {
	var x = { 4 42 18 78 };
	return (x@2):number;
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(18, result);
		}

		[Test]
		public void list_index_set()
		{
			var script = @"
test _ : number {
	var x = { 0 0 0 0 };
	let x@2 = 42;
	return (x@2):number;
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(42, result);
		}
	}
}
