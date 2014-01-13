﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NinbotTests
{
    [TestFixture]
    public class Math
    {
        [Test]
        public void add()
        {
			TestHelper.QuickTest("return 5 + 3", 8);
			TestHelper.QuickTest("return 4 + 2 + 3", 9);
		}

		[Test]
		public void sub()
		{
			TestHelper.QuickTest("return 5 - 3", 2);
		}

		[Test]
		public void modulus()
		{
			TestHelper.QuickTest("return 5 % 3", 2);
		}

		[Test]
		public void multiply()
		{
			TestHelper.QuickTest("return 4 * 0.25", 1);
		}

    }

}