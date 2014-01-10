using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NinbotTests
{
    public class TestHelper
    {
        public static void RunSimpleTest(String script, Object expectedResult)
        {
            Assert.AreEqual(expectedResult, RunSimpleTest(script));
        }

        public static Object RunSimpleTest(String script)
        {
			Console.WriteLine("Test script: " + script);

			var declaration = Ninbot.Compile.CompileDeclaration(script);
			var context = new Ninbot.VirtualMachine.ExecutionContext(new Ninbot.VirtualMachine.ScriptObject(),
				new Ninbot.VirtualMachine.CodeContext(declaration.Instructions, 0));
			Ninbot.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			return context.R;
        }
    }
}
