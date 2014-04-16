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
			//            Assert.AreEqual(8, TestHelper.CallTestFunction(@"
			//test _ : number {
			//	var t = 0;
			//	foreach x int
					int total_calls = 0;

			//var environment = new EtcScriptLib.VirtualMachine.ScriptObject();
			//environment.SetProperty("foo", new EtcScriptLib.VirtualMachine.NativeFunction((c, args) =>
			//    {
			//        return new List<Object>(new Object[] { 1, 2, 3 });
			//    }));
			//environment.SetProperty("bar", new EtcScriptLib.VirtualMachine.NativeFunction((c, args) =>
			//    {
			//        total_calls += 1;
			//        return null;
			//    }));

			var script = @"
test foo {
	foreach x in :[foo] {
		:[bar x];
	}
}
";

			Console.WriteLine("Test script: " + script);

			var declaration = EtcScriptLib.Compile.CompileDeclaration(script,
				 (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });
			var context = new EtcScriptLib.VirtualMachine.ExecutionContext(EtcScriptLib.VirtualMachine.CodeContext.Empty);
			declaration.MakeInvokableFunction().Invoke(context, new List<Object>());
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
			Assert.AreEqual(9, TestHelper.CallTestFunction(@"
test _ : number {
	var t = 0;
	foreach x from 0 to 8 {
		let t = t + 1;
	}
	return t;
}"));
		}


		[Test]
		public void while_loop()
		{
			Assert.AreEqual(5, TestHelper.CallTestFunction(@"
test _ : number {
	var t = 0;
	while (t < 5) {
		let t = t + 1;
	}
	return t;
}"));
		}

	}
}