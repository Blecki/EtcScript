using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class Compilation
    {
        [Test]
        public void environment_compiles_trivial_script()
        {
			TestHelper.CompileTestAssertNoErrors("test _ {}");
        }

		[Test]
		public void negative_number()
		{
			TestHelper.CompileTestAssertNoErrors(@"test _ { var x = -5; }");
			TestHelper.CompileTestAssertNoErrors(@"test _ { var x = 5 - 4; }");
			TestHelper.CompileTestAssertNoErrors(@"test _ { var x = 5 * -4; }");
			TestHelper.CompileShouldError(@"test _ { var x = (5 -4); }");
		}

		[Test]
		public void comments()
		{
			TestHelper.CompileTestAssertNoErrors(@"test foo #comment comment comment
	{ var x = 4; } #comment comment comment");
		}

		[Test]
		public void case_irrelevent()
		{
			TestHelper.CompileTestAssertNoErrors(@"
test foo {
	var x = 4;
	VaR y = 7;
}");
		}        
    }

}