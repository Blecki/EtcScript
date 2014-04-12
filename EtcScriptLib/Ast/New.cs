using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class New : Node
	{
		public String Typename;

		public New(Token Source) : base(Source) 
		{
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType(Typename);
			if (ResultType == null) throw new CompileError("Unable to find type with name '" + Typename + "'", Source);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.AddInstructions("ALLOC_RSO NEXT " + WriteOperand(Destination), ResultType.Size);
		}

	}
}
