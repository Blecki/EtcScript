using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class LambdaFunction : InvokeableFunction
    {
        public String Name = null;
		public InstructionList Body;
		public ScriptObject Scope;
		public List<String> Arguments;
		private static InstructionList Cleanup;

        public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
        {
			if (Cleanup == null)
				Cleanup = new InstructionList(
					"SET_FRAME POP",
					"CONTINUE POP");

			VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);
			VirtualMachine.SetOperand(Operand.PUSH, context.Frame, context);
			VirtualMachine.SetOperand(Operand.PUSH, new CodeContext(Cleanup, 0), context);
			context.Frame = new ScriptObject("@parent", Scope);
			context.CurrentInstruction = new CodeContext(Body, 0);

			for (int i = 0; i < Arguments.Count && i < arguments.Count - 1; ++i)
				context.Frame.SetProperty(Arguments[i], arguments[i + 1]);

			return InvokationResult.Success;
        }

		public static LambdaFunction CreateLambda(String Name, InstructionList Body, ScriptObject Scope, List<String> Arguments)
		{
			var r = new LambdaFunction();
			r.Name = Name;
			r.Body = Body;//new InstructionList(
				//new InPlace(Body),
				//"CONTINUE POP");
			r.Scope = Scope;
			r.Arguments = Arguments;
			return r;
		}

    }
}
