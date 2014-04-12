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
		public String Typename;

		public LocalDeclaration(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			if (String.IsNullOrEmpty(Typename))
				ResultType = Type.Generic;
			else
			{
			ResultType = Scope.FindType(Typename);
			if (ResultType == null) throw new CompileError("Could not find type '" + Typename + "'.", Source);
			}

			if (Value != null)
			{
				Value = Value.Transform(Scope);
				if (Value.ResultType == Type.Void) throw new CompileError("Can't assign void to variable", Source);
				ResultType = Value.ResultType;
			}
			Variable = Scope.NewLocal(Name, ResultType);
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
