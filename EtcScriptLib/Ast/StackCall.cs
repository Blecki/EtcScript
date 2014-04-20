using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class StackCall : Statement
	{
		public List<Node> Arguments;
		public Declaration Function;

		public StackCall(Token Source, Declaration Function, List<Node> Arguments) : base(Source) 
		{
			this.Function = Function;
			this.Arguments = Arguments;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Function.ReturnType;
			Arguments = new List<Node>(Arguments.Select(n => n.Transform(Scope)));
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			foreach (var arg in Arguments)
				arg.Emit(into, OperationDestination.Stack);
			into.AddInstructions("STACK_INVOKE NEXT", Function.MakeInvokableFunction());
			if (Arguments.Count > 0) into.AddInstructions("CLEANUP NEXT", Arguments.Count);
			if (Destination != OperationDestination.R && Destination != OperationDestination.Discard)
				into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
		}

	}
}
