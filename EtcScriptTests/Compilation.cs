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
			var declaration = EtcScriptLib.Compile.CompileDeclaration(@"test foo {}
			", (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
            Assert.AreNotEqual(null, declaration);
        }

		[Test]
		public void interpret_semicolon_as_newline_with_equal_tabs()
		{
			TestHelper.CompileTestAssertNoErrors(@"test foo
	{ let x = 5; return x; }");
			TestHelper.CompileTestAssertNoErrors(@"
	test foo
	{
		foreach x in y
		{
			let x = 5; 
			return x;
		}
	}");
		}

		[Test]
		public void negative_number()
		{
			TestHelper.CompileTestAssertNoErrors(@"test foo { let x = -5; }");
			TestHelper.CompileTestAssertNoErrors(@"test foo { let x = 5 - 4; }");
			TestHelper.CompileTestAssertNoErrors(@"test foo { let x = 5 * -4; }");
			TestHelper.CompileShouldError(@"test foo { let x = (5 -4); }");
		}

		[Test]
		public void comments()
		{
			TestHelper.CompileTestAssertNoErrors(@"test foo #comment comment comment
	{ let x = 4; } #comment comment comment");
		}

		[Test]
		public void case_irrelevent()
		{
			TestHelper.CompileTestAssertNoErrors(@"
test foo {
	let x = 4;
	LeT y = 7;
}");
		}        
    }

}