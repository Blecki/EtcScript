using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class FunctionCall : Statement
	{
		public List<Node> Arguments;
		public VirtualMachine.InvokeableFunction Function;

		public FunctionCall(Token Source, VirtualMachine.InvokeableFunction Function, List<Node> Arguments) : base(Source) 
		{
			this.Function = Function;
			this.Arguments = Arguments;
		}

		public override Node Transform(ParseScope Scope)
		{
			if (Function.IsStackInvokable) return new StackCall(Source, Function, Arguments).Transform(Scope);
			else return new CompatibleCall(Source, new AssembleList(Source, Arguments)).Transform(Scope);
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}

	}
}
