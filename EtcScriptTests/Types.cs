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

		[Test]
		public void initializer()
		{
			var script = @"
type foo
{
	var x:number;
	var y:number;
}

test _ : number {
	var a = new foo {
		let x = 5;
		let y = 6;
	};

	return a.x * a.y;
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(30, result);
		}

		[Test]
		public void inheritance()
		{
			var script = @"
type foo { var a:number; }
type bar : foo { var b:number; }

test _ : number {
	var x = new bar { let a = 4; let b = 5; };
	return x.a * x.b;
}";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(20, result);
		}

		private class T
		{
			public int X;
			public int Y;

			public T(int X, int Y)
			{
				this.X = X;
				this.Y = Y;
			}
		}

		[Test]
		public void generic_system_getter()
		{
			var script = @"
test _ : number {
	var t = [make t];
	return (t.x * t.y):number;
}";
			var result = TestHelper.CallTestFunction(script, e =>
				{
					e.AddSystemType("T");
					e.AddSystemMacro("make t : T", (c, l) => { return new T(42, 87); });
					e.AddSystemMacro("get (x:string) from (y:T) : generic", (c, l) =>
					{							
						var v = l[1] as T;
						var n = l[0].ToString().ToUpper();

						if (n == "X")
							return v.X;
						else if (n == "Y")
							return v.Y;
						else
							EtcScriptLib.VirtualMachine.VirtualMachine.Throw("'" + n + "' is not a member of T.", c);
						return 0;
					});
				});

			Assert.AreEqual(42 * 87, result);
		}

		[Test]
		public void explicit_system_getter()
		{
			var script = @"
test _ : number {
	var t = [make t];
	return (t.x * t.y):number;
}";
			var result = TestHelper.CallTestFunction(script, e =>
			{
				e.AddSystemType("T");
				e.AddSystemMacro("make t : T", (c, l) => { return new T(42, 87); });
				e.AddSystemMacro("get x from (t:T) : number", (c, l) =>
				{
					return (l[0] as T).X;
				});
				e.AddSystemMacro("get y from (t:T) : number", (c, l) =>
				{
					return (l[0] as T).Y;
				});
			});

			Assert.AreEqual(42 * 87, result);
		}


		[Test]
		public void generic_system_setter()
		{
			var script = @"
test _ : number {
	var t = [make t];
	let t.x = 5;
	let t.y = 6;
	return (t.x * t.y):number;
}";
			var result = TestHelper.CallTestFunction(script, e =>
			{
				e.AddSystemType("T");
				e.AddSystemMacro("make t : T", (c, l) => { return new T(42, 87); });
				e.AddSystemMacro("get (x:string) from (y:T) : generic", (c, l) =>
				{
					var v = l[1] as T;
					var n = l[0].ToString().ToUpper();

					if (n == "X")
						return v.X;
					else if (n == "Y")
						return v.Y;
					else
						EtcScriptLib.VirtualMachine.VirtualMachine.Throw("'" + n + "' is not a member of T.", c);
					return 0;
				});
				e.AddSystemMacro("set (x:string) on (y:T) to (z:generic)", (c, l) =>
				{
					var v = l[1] as T;
					var n = l[0].ToString().ToUpper();
					if (n == "X")
						v.X = Convert.ToInt32(l[2]);
					else if (n == "Y")
						v.Y = Convert.ToInt32(l[2]);
					else
						EtcScriptLib.VirtualMachine.VirtualMachine.Throw("'" + n + "' is not a member of T.", c);
					return null;
				});
			});

			Assert.AreEqual(30, result);
		}

		[Test]
		public void explicit_system_setter()
		{
			var script = @"
test _ : number {
	var t = [make t];
	let t.x = 5;
	let t.y = 6;
	return (t.x * t.y):number;
}";
			var result = TestHelper.CallTestFunction(script, e =>
			{
				e.AddSystemType("T");
				e.AddSystemMacro("make t : T", (c, l) => { return new T(42, 87); });
				e.AddSystemMacro("get x from (t:T) : number", (c, l) =>
				{
					return (l[0] as T).X;
				});
				e.AddSystemMacro("get y from (t:T) : number", (c, l) =>
				{
					return (l[0] as T).Y;
				});
				e.AddSystemMacro("set x on (t:T) to (a:number)", (c, l) =>
				{
					(l[0] as T).X = Convert.ToInt32(l[1]);
					return null;
				});
				e.AddSystemMacro("set y on (t:T) to (a:number)", (c, l) =>
				{
					(l[0] as T).Y = Convert.ToInt32(l[1]);
					return null;
				});
			});

			Assert.AreEqual(30, result);
		}

	}
}