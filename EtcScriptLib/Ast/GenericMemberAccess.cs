using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class GenericMemberAccess : Node, IAssignable
	{
		public Node Object;
		public String Name;

		public GenericMemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("LOOKUP_MEMBER STRING R " + Node.WriteOperand(Destination), into.AddString(Name));
		}

		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("SET_MEMBER POP NEXT R", into.AddString(Name));
		}
	}
}
