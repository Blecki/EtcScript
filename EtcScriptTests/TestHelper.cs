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
			RunSimpleTest(@"test foo : number { " + script + " }", expectedResult);
		}

		public static void CompileTestAssertNoErrors(String script)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.False(wasError);
		}

		public static void CompileShouldError(String script)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			Console.WriteLine("Test script: " + script);
			bool wasError = false;
			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); wasError = true; return EtcScriptLib.ErrorStrategy.Continue; });
			Assert.True(wasError);
		}

        public static Object RunSimpleTest(String script)
        {
			Console.WriteLine("Test script: " + script);

			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				(s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(EtcScriptLib.VirtualMachine.CodeContext.Empty);
			var invokable = declaration.MakeInvokableFunction();
			invokable.Invoke(context, new List<Object>(new Object[]{invokable}));
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.R == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.R.ToString());
			return context.R;
        }

		public static EtcScriptLib.VirtualMachine.InvokeableFunction BuildTestEnvironment(
			String script, 
			Action<EtcScriptLib.Environment> AdditionalSetup = null)
		{
			Console.WriteLine("Test script: " + script);
			var environment = EtcScriptLib.Environment.CreateStandardEnvironment();
			environment.AddSystemMacro("fail", (c, a) => { Assert.IsTrue(false); return null; });
			if (AdditionalSetup != null) AdditionalSetup(environment);
			var declarations = environment.Build(script, (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Abort; });
			EtcScriptLib.VirtualMachine.InvokeableFunction r = null;
			foreach (var declaration in declarations)
				r = declaration.MakeInvokableFunction();
			
			return r;
		}
		
		public static Object CallTestFunction(
			String script,
			Action<EtcScriptLib.Environment> AdditionalSetup = null)
		{
			EtcScriptLib.Compile.Debug = true;
			EtcScriptLib.Compile._DebugWrite = Console.Write;

			var func = BuildTestEnvironment(script, AdditionalSetup);
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(EtcScriptLib.VirtualMachine.CodeContext.Empty);

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
