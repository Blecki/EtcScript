using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class Math
    {
        [Test]
        public void add()
        {
			TestHelper.MathTest("return 5 + 3;", 8);
			TestHelper.MathTest("return 4 + 2 + 3;", 9);
		}

		[Test]
		public void sub()
		{
			TestHelper.MathTest("return 5 - 3;", 2);
		}

		[Test]
		public void modulus()
		{
			TestHelper.MathTest("return 5 % 3;", 2);
		}

		[Test]
		public void multiply()
		{
			TestHelper.MathTest("return 4 * 0.25;", 1);
		}

		[Test]
		public void precedence()
		{
			TestHelper.MathTest("return 4 * 2 + 3;", 11);
			TestHelper.MathTest("return 1 + 2 * 3 + 4;", 11);
		}
    }

}