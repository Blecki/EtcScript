using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Box : Node
	{
		public Node Value;

		public Box(Token Source, Node Value) : base(Source) 
		{
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType("BOXED");
			Value = Value.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Value.Emit(into, OperationDestination.R);
			into.AddInstructions("ALLOC_RSO NEXT PUSH", 3,
				"STORE_RSO_M NEXT PEEK NEXT", ResultType.ID, 0,
				"STORE_RSO_M NEXT PEEK NEXT", Value.ResultType.ID, 1,
				"STORE_RSO_M R PEEK NEXT", 2);
			if (Destination != OperationDestination.Stack)
				into.AddInstructions("MOVE POP " + WriteOperand(Destination));
		}

		public static void EmitUnbox(
			VirtualMachine.InstructionList into, 
			OperationDestination Source, 
			OperationDestination Destination)
		{
			into.AddInstructions("LOAD_RSO_M " + ReadOperand(Source) + " NEXT " + WriteOperand(Destination), 2);
		}
	}

}
