using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class Lambda
    {
        [Test]
        public void lambda_test()
        {
			var result = TestHelper.CallTestFunction(@"
test _ : number {
	var f = lambda (a:number) : number { return a * a; };
	return (:[f 2]):number;
}");

			Assert.AreEqual(result, 4);
		}

		[Test]
		public void lambda_capture()
		{
			var result = TestHelper.CallTestFunction(@"
macro make lambda (x) : generic {
	var f0 = lambda (a) : generic {
		return lambda (b) : generic {
			return a * b * x;
		};
	};
	return :[f0 4];
}

test _ : generic {
	var f = [make lambda 2];
	return :[f 3];
}");

			Assert.AreEqual(result, 24);
		}        
    }

}