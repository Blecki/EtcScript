using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Cast : Node
	{
		public Node Value;
		public String Typename;

		public Cast(Token Source, Node Value, String Typename) : base(Source) 
		{
			this.Value = Value;
			this.Typename = Typename;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType(Typename);
			if (ResultType == null) throw new CompileError("Could not find type '" + Typename + "'.", Source);
			Value = Value.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Value.Emit(into, Destination);
		}

	}
}
