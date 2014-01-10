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
			");
            Assert.AreNotEqual(null, declaration);
        }

        
    }

}