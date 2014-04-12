using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class LambdaFunction : InvokeableFunction
    {
        public String Name = null;
		public CodeContext EntryPoint;
		public List<String> Arguments;

		public override bool IsStackInvokable { get { return true; } }

		public override void StackInvoke(ExecutionContext context)
		{
			context.CurrentInstruction = EntryPoint;
		}

        public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
        {
			if (arguments.Count > 1)
			{
				var cleanup = new InstructionList("CLEANUP NEXT", arguments.Count - 1, "CONTINUE POP");
				VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);
				for (int i = 1; i < arguments.Count; ++i)
					VirtualMachine.SetOperand(Operand.PUSH, arguments[i], context);
				VirtualMachine.SetOperand(Operand.PUSH, new CodeContext(cleanup, 0), context);
			}
			else
				VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);

			context.CurrentInstruction = EntryPoint;

			return InvokationResult.Success;
        }

		public static LambdaFunction CreateLambda(String Name, CodeContext Body, List<String> Arguments)
		{
			var r = new LambdaFunction();
			r.Name = Name;
			r.EntryPoint = Body;
			r.Arguments = Arguments;
			return r;
		}

    }
}
