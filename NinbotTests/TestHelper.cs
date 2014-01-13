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

		public static void QuickTest(String script, Object expectedResult)
		{
			RunSimpleTest(@"activity foo
	" + script, expectedResult);
		}

		public static void CompileTestAssertNoErrors(String script)
		{
			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			var declaration = Ninbot.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return Ninbot.ErrorStrategy.Continue; });
			Assert.False(wasError);
		}

		public static void CompileShouldError(String script)
		{
			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			var declaration = Ninbot.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return Ninbot.ErrorStrategy.Continue; });
			Assert.True(wasError);
		}

        public static Object RunSimpleTest(String script)
        {
			Console.WriteLine("Test script: " + script);

			var declaration = Ninbot.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); return Ninbot.ErrorStrategy.Continue; });
			var context = new Ninbot.VirtualMachine.ExecutionContext(new Ninbot.VirtualMachine.ScriptObject(),
				new Ninbot.VirtualMachine.CodeContext(declaration.Instructions, 0));
			Ninbot.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == Ninbot.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.Peek == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.Peek.ToString());
			return context.Peek;
        }
    }
}
