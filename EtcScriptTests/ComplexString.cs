using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class ComplexString
    {
		[Test]
		public void compile_complex_string()
		{
			var result = TestHelper.CallTestFunction(@"
macro cstr (x:number) (y:number) : generic {
	var complex = 5;
	return $""This [x] is [y] a [complex] string"";
}

test _ : string {
	return (:[[cstr 1 2]]):string;
}");

			Assert.AreEqual(result, "This 1 is 2 a 5 string");
		}

		[Test]
		public void conversion()
		{
			var result = TestHelper.CallTestFunction(@"
macro cstr (x:number) (y:number) : complexstring {
	var complex = 5;
	return $""This [x] is [y] a [complex] string"";
}

macro convert (s:complexstring) to string : string {
	return (:[s]):string;
}

test _ : string {
	return [cstr 1 2];
}");

			Assert.AreEqual(result, "This 1 is 2 a 5 string");
		}     
    }

}