using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class OperatorOverloading
    {
		[Test]
		public void no_operator()
		{
			ErrorsReported.throws_error(@"
type vector {
	var x:number;
	var y:number;
}

test _ : vector {
	var a = new vector {};
	var b = new vector {};
	return a + b;
}");
		}

		[Test]
		public void vector_ops()
		{
			var script = @"
type vector {
	var x:number;
	var y:number;
}

macro (a:vector) + (b:vector) : vector {
	return new vector {
		let x = a.x + b.x;
		let y = a.y + b.y;
	};
}

test _ : number {
	var a = new vector { let x = 1; let y = 2; };
	var b = new vector { let x = 4; let y = 7; };
	var c = a + b;
	return c.x * c.y; 
}";

			var result = TestHelper.CallTestFunction(script);

			Assert.AreEqual(result, (1 + 4) * (2 + 7));
		}

    }

}