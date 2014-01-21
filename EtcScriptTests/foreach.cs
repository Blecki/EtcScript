using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class Foreach
    {
        [Test]
        public void foreach_in_loop()
        {
			int total_calls = 0;

			var environment = new EtcScriptLib.VirtualMachine.ScriptObject();
			environment.SetProperty("foo", new EtcScriptLib.VirtualMachine.NativeFunction((c, args) =>
				{
					return new List<Object>(new Object[] { 1, 2, 3 });
				}));
			environment.SetProperty("bar", new EtcScriptLib.VirtualMachine.NativeFunction((c, args) =>
				{
					total_calls += 1;
					return null;
				}));

			var script = @"activity foo
	foreach x in [foo]
		bar x
";
			
			Console.WriteLine("Test script: " + script);

			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				 (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(environment,
				new EtcScriptLib.VirtualMachine.CodeContext(declaration.Instructions, 0));
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.Peek == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.Peek.ToString());

			Assert.AreEqual(total_calls, 3);
	    }


		[Test]
		public void foreach_from_loop()
		{
			int total_calls = 0;

			var environment = new EtcScriptLib.VirtualMachine.ScriptObject();
			environment.SetProperty("bar", new EtcScriptLib.VirtualMachine.NativeFunction((c, args) =>
			{
				Assert.AreEqual(total_calls, args[0]);
				total_calls += 1;
				return null;
			}));

			var script = @"activity foo
	foreach x from 0 to 5 exclusive
		bar x
";

			Console.WriteLine("Test script: " + script);

			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				 (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(environment,
				new EtcScriptLib.VirtualMachine.CodeContext(declaration.Instructions, 0));
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(context);
			if (context.ExecutionState == EtcScriptLib.VirtualMachine.ExecutionState.Error)
				Console.WriteLine("Error:" + context.ErrorObject.ToString());

			if (context.Peek == null) Console.WriteLine("NULL");
			else Console.WriteLine(context.Peek.ToString());

			Assert.AreEqual(6, total_calls);
		}

		
    }

}