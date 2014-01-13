using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NinbotTests
{
    [TestFixture]
    public class Compilation
    {
        [Test]
        public void environment_compiles_trivial_script()
        {
			var declaration = Ninbot.Compile.CompileDeclaration(@"Activity foo
			", (s) => { Console.WriteLine(s); return Ninbot.ErrorStrategy.Continue; });
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

        
    }

}