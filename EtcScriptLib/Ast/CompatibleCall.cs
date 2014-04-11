using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class CompatibleCall : Statement
	{
		public AssembleList Parameters;

		public CompatibleCall(Token Source, AssembleList Parameters)
			: base(Source) 
		{
			this.Parameters = Parameters;
		}

		public override Node Transform(ParseScope Scope)
		{
			Parameters = Parameters.Transform(Scope) as AssembleList;
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Parameters.Emit(into, OperationDestination.Top);
			into.AddInstructions("INVOKE POP");
			if (Destination != OperationDestination.R && Destination != OperationDestination.Discard)
				into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Invoke");
			Parameters.Debug(depth + 1);
		}
	}
}
