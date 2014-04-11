using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class LocalDeclaration : Statement
	{
		public String Name;
		public Node Value;
		public Variable Variable;

		public LocalDeclaration(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			if (Value != null) Value = Value.Transform(Scope);
			Variable = Scope.NewLocal(Name);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Value != null)
			{
				Value.Emit(into, OperationDestination.Stack);
			}
			else
			{
				into.AddInstructions("MOVE NEXT PUSH", 0); //Just make room on the stack for it...
			}
		}

	}
}
