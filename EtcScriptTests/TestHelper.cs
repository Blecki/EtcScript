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
				declaration.GetEntryPoint());
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.R == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.R.ToString());
			return context.R;
        }

		public static EtcScriptLib.Environment BuildEnvironment(String script)
		{
			Console.WriteLine("Test script: " + script);
			var environment = EtcScriptLib.Environment.CreateStandardEnvironment();
			var declarations = environment.Build(script, (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Abort; });
			foreach (var declaration in declarations)
				environment.GlobalScope.SetProperty(declaration.UsageSpecifier, declaration.MakeInvokableFunction());
			return environment;
		}

		public static Object CallEnvironmentFunction(String script, String function)
		{
			var scope = BuildEnvironment(script);
			var func = scope.GlobalScope.GetOwnProperty(function) as EtcScriptLib.VirtualMachine.InvokeableFunction;
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(scope.GlobalScope,
				new EtcScriptLib.VirtualMachine.CodeContext(new EtcScriptLib.VirtualMachine.InstructionList(), 0));
			func.Invoke(context, new List<object>());
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.R == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.R.ToString());
			return context.R;
		}
    }
}
