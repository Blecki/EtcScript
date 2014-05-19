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
			Assert.AreEqual(expectedResult, CallTestFunction(script));
        }

		public static void RunSimpleTest(String script)
		{
			CallTestFunction(script);
		}

		public static void MathTest(String script, Object expectedResult)
		{
			RunSimpleTest(@"test _ : number { " + script + " }", expectedResult);
		}

		public static void CompileTestAssertNoErrors(String script)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			var environment = new EtcScriptLib.Environment();
			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			environment.Build(script, 
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.False(wasError);
		}

		public static void CompileShouldError(String script)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			var environment = new EtcScriptLib.Environment();
			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			environment.Build(script,
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.True(wasError);
		}

		public static Object CallTestFunction(
			String script,
			Action<EtcScriptLib.Environment> AdditionalSetup = null)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			Console.WriteLine("Test script: " + script);
			var environment = new EtcScriptLib.Environment();
			environment.AddSystemMacro("fail", (c, a) => { Assert.IsTrue(false); return null; });
			if (AdditionalSetup != null) AdditionalSetup(environment);
			var testFunctions = environment.Build(script, s => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Abort; });
			Assert.IsTrue(testFunctions.Count > 0);
			var context = environment.CreateExecutionContext(EtcScriptLib.VirtualMachine.ExecutionLocation.Empty);
			var func = testFunctions[0].MakeInvokableFunction();
			var argList = new List<Object>();
			argList.Add(func);
			func.Invoke(context, argList);
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
			{
				Console.WriteLine("Error:" + context.ErrorObject.ToString());
				Assert.IsTrue(false);
			}

			if (context.R == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.R.ToString());
			return context.R;
		}
    }
}
