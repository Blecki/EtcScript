using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public partial class Euler
    {
		

		//If we list all the natural numbers below 10 that are multiples of 3 or 5, we get 3, 5, 6 and 9. 
		//The sum of these multiples is 23.

		//Find the sum of all the multiples of 3 or 5 below 1000.

		[Test]
		public void _001_multiples_of_3_and_5()
		{
			//C# solution
			var total = 0;
			for (int i = 1; i < 1000; ++i)
				if ((i % 3) == 0 || (i % 5) == 0) total += i;

			var script = @"
test _ : number
{
	var total = 0;
	foreach i from 1 to 999
	{
		if (i % 3 == 0)
		{
			let total = total + i;
		}
		else if (i % 5 == 0)
		{
			let total = total + i;
		}
	}
	return total;
}
";

			var result = TestHelper.CallTestFunction(script);
			Assert.AreEqual(total, result);

		}        
    }

}