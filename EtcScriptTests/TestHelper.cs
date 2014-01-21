using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
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
			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.False(wasError);
		}

		public static void CompileShouldError(String script)
		{
			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.True(wasError);
		}

        public static Object RunSimpleTest(String script)
        {
			Console.WriteLine("Test script: " + script);

			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(new EtcScriptLib.VirtualMachine.ScriptObject(),
				new EtcScriptLib.VirtualMachine.CodeContext(declaration.Instructions, 0));
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.R == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.R.ToString());
			return context.R;
        }
    }
}
