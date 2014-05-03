using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class StringLiteral : Node
	{
		public String Value;

		public StringLiteral(Token Source, String Value)
			: base(Source)
		{
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType("STRING");
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.AddInstructions("MOVE STRING " + Node.WriteOperand(Destination), into.AddString(Value));
		}

		public override string ToString()
		{
			return "\"" + Value + "\"";
		}
	}
}
