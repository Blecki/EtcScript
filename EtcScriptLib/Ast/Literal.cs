using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Literal : Node
	{
		public Object Value;
		public String Typename;

		public Literal(Token Source, Object Value, String Typename) : base(Source) 
		{
			this.Value = Value;
			this.Typename = Typename;
		}

		public override Node Transform(ParseScope Scope)
		{
			if (String.IsNullOrEmpty(Typename)) ResultType = Type.Generic;
			else
			{
				ResultType = Scope.FindType(Typename);
				if (ResultType == null) throw new CompileError("Could not find type '" + Typename + "'.", Source);
			}
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.AddInstructions("MOVE NEXT " + Node.WriteOperand(Destination), Value);
		}

		public override string ToString()
		{
			return "LIT<<" + (Value == null ? "NULL" : Value.ToString()) + ">>";
		}
	}
}
