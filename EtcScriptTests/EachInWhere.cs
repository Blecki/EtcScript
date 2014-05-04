using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
	[TestFixture]
	public class EachInWhere
	{
		[Test]
		public void each_x_in_where()
		{
			Assert.AreEqual(3, TestHelper.CallTestFunction(@"
test _ : number {
	return [length of [each X in { 0 1 2 3 4 } where ((X:number % 2) == 0)]];
}"));
		}

	}
}