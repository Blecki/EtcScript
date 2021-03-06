﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class ErrorsReported
    {
		public static void throws_error(String script)
		{
			bool errorCaught = false;

			Console.WriteLine("Script: " + script);
			var environment = new EtcScriptLib.Environment();
			environment.Build(script, (s) =>
			{
				Console.WriteLine("Error: " + s);
				errorCaught = true;
				return EtcScriptLib.ErrorStrategy.Abort;
			});

			if (!errorCaught)
			{
				Console.WriteLine("No errors.");
				Assert.Fail();
			}
		}

        [Test]
        public void errors_reported()
        {
			throws_error(@"test foo
	let bar = (one two three)");
			throws_error(@"test foo
	let bar = one.(three.four)");
        }

        
    }

}