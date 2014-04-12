using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class Types
	{
		[Test]
		public void types_compile()
		{
			var script = @"
type foo
{
	var x;
	var y;
}

test _ {
	return 6;	
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(6, result);
		}

		[Test]
		public void create_types()
		{
			var script = @"
type foo
{
	var x;
	var y;
}

test _ {
	var a = new foo;
	return 6;	
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(6, result);
		}

		[Test]
		public void access_members()
		{
			var script = @"
type foo
{
	var x;
	var y;
}

test _ {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return a.y;	
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(8, result);
		}

		[Test]
		public void argument_types()
		{
			var script = @"
macro bar (a:foo) 
{
	return a.y;
} : number

type foo
{
	var x;
	var y;
}

test _ {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return [bar a];
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(8, result);
		}

		[Test]
		public void member_types()
		{
			var script = @"
macro bar (a:number) 
{
	return a;
} : number

type foo
{
	var x:number;
	var y:number;
}

test _ {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return [bar a.y];
}
";

			var result = TestHelper.CallEnvironmentFunction(script, "test");
			Assert.AreEqual(8, result);
		}
	}
}