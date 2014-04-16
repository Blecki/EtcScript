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

test _ : number {
	return 6;	
}
";

			var result = TestHelper.CallTestFunction(script);
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

test _ : number {
	var a = new foo;
	return 6;	
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(6, result);
		}

		[Test]
		public void access_members()
		{
			var script = @"
type foo
{
	var x:number;
	var y:number;
}

test _ : number {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return a.y;	
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(8, result);
		}

		[Test]
		public void argument_types()
		{
			var script = @"
macro bar (a:foo) : number 
{
	return a.y;
}

type foo
{
	var x:number;
	var y:number;
}

test _ : number {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return [bar a];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(8, result);
		}

		[Test]
		public void member_types()
		{
			var script = @"
macro bar (a:number) : number
{
	return a;
}

type foo
{
	var x:number;
	var y:number;
}

test _ : number {
	var a = new foo;
	let a.x = 4;
	let a.y = a.x * 2;
	return [bar a.y];
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(8, result);
		}
	}
}