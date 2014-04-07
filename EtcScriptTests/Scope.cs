using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class Scope
	{
		[Test]
		public void outside_scope_inacessible()
		{
			var script = @"

push scope;

macro foo {}

pop scope;

test _ {
	foo;
}
";
			TestHelper.CompileShouldError(script);
		}
	}
}