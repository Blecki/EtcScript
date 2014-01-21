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
			var declaration = EtcScriptLib.Compile.CompileDeclaration(@"Activity foo
			", (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
            Assert.AreNotEqual(null, declaration);
        }

		[Test]
		public void interpret_semicolon_as_newline_with_equal_tabs()
		{
			TestHelper.CompileTestAssertNoErrors(@"activity foo
	let x = 5; return x");
			TestHelper.CompileTestAssertNoErrors(@"activity foo
	foreach x in y
		let x = 5; return x");
		}

		[Test]
		public void negative_number()
		{
			TestHelper.CompileTestAssertNoErrors(@"activity foo
	let x = -5");
			TestHelper.CompileTestAssertNoErrors(@"activity foo
	let x = 5 - 4");
			TestHelper.CompileTestAssertNoErrors(@"activity foo
	let x = 5 * -4");
			TestHelper.CompileShouldError(@"activity foo
	let x = 5-4");
		}

		[Test]
		public void comments()
		{
			TestHelper.CompileTestAssertNoErrors(@"activity foo #comment comment comment
	let x = 4 #comment comment comment");
		}

        
    }

}