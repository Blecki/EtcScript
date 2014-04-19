using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class LambdaFunction : InvokeableFunction
    {
		public String Name;
		public CodeContext CleanupPoint;
		public CodeContext EntryPoint;
		public int ArgumentCount;

		public override bool IsStackInvokable { get { return true; } }

		public override void StackInvoke(ExecutionContext context)
		{
			context.CurrentInstruction = EntryPoint;
		}

        public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
        {
			if (arguments.Count != ArgumentCount + 1) 
				throw new InvalidProgramException("Expected " + (ArgumentCount + 1) + " arguments, got " + arguments.Count);

			if (ArgumentCount > 0)
			{
				VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);
				for (int i = 0; i < ArgumentCount; ++i)
					VirtualMachine.SetOperand(Operand.PUSH, arguments[i + 1], context);
				VirtualMachine.SetOperand(Operand.PUSH, CleanupPoint, context);
			}
			else
				VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);

			context.CurrentInstruction = EntryPoint;

			return InvokationResult.Success;
        }

		public static LambdaFunction CreateLambda(String Name, CodeContext CleanupPoint, CodeContext EntryPoint, int ArgumentCount)
		{
			var r = new LambdaFunction();
			r.CleanupPoint = CleanupPoint;
			r.EntryPoint = EntryPoint;
			r.ArgumentCount = ArgumentCount;
			r.Name = Name;
			return r;
		}

		public static TrueLambdaFunction CreateTrueLambda(CodeContext Source, RuntimeScriptObject CapturedVariables)
		{
			return new TrueLambdaFunction
			{
				CapturedVariables = CapturedVariables
			};
		}

		public override string ToString()
		{
			return "Lambda { " + Name + " }";
		}
    }

	public class TrueLambdaFunction : InvokeableFunction
	{
		public RuntimeScriptObject CapturedVariables;

		public override bool IsStackInvokable { get { return false; } }

		public override void StackInvoke(ExecutionContext context)
		{
			throw new InvalidOperationException();
		}

		public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
		{
			var expectedParameterCount = (CapturedVariables.Data[CapturedVariables.Data.Count - 1] as int?).Value;
			if (arguments.Count != expectedParameterCount)
				return InvokationResult.Failure("Incorrect number of arguments. Expected "
					+ (expectedParameterCount) + " got "
					+ (arguments.Count));

			VirtualMachine.SetOperand(Operand.PUSH, context.CurrentInstruction, context);
			for (int i = 0; i < expectedParameterCount - 1; ++i)
				VirtualMachine.SetOperand(Operand.PUSH, arguments[i + 1], context);
			VirtualMachine.SetOperand(Operand.PUSH, CapturedVariables, context);
			VirtualMachine.SetOperand(Operand.PUSH, new CodeContext(context.CurrentInstruction.Code,
				(CapturedVariables.Data[CapturedVariables.Data.Count - 3] as int?).Value), context);

			context.CurrentInstruction = new CodeContext(context.CurrentInstruction.Code,
				(CapturedVariables.Data[CapturedVariables.Data.Count - 2] as int?).Value);

			return InvokationResult.Success;
		}

		public override string ToString()
		{
			return "TrueLambda";
		}
	}
}
