using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NinbotTests
{
    [TestFixture]
    public class ErrorsReported
    {
		private void throws_error(String script)
		{
			bool errorCaught = false;

			Console.WriteLine("Script: " + script);
			Ninbot.Compile.Build(script, (s) =>
			{
				Console.WriteLine("Error: " + s);
				errorCaught = true;
				return Ninbot.ErrorStrategy.Abort;
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
			throws_error("activity");
			throws_error(@"activity foo
	let bar = one two three");
			throws_error(@"activity foo
	let bar = one.(three.four)");
        }

        
    }

}