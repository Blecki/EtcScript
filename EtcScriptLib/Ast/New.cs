using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class New : Node
	{
		public String Typename;
		public List<Initializer> Initializers;

		public New(Token Source) : base(Source) 
		{
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType(Typename);
			if (ResultType == null) throw new CompileError("Unable to find type with name '" + Typename + "'", Source);

			if (Initializers != null)
			{
				foreach (var initializer in Initializers)
				{
					initializer.ObjectType = ResultType;
					initializer.Transform(Scope);
				}
			}

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Initializers != null)
			{
				into.AddInstructions("ALLOC_RSO NEXT PUSH", ResultType.Size);
				foreach (var initializer in Initializers)
					initializer.Emit(into, OperationDestination.Discard);
				if (Destination != OperationDestination.Stack)
					into.AddInstructions("MOVE POP " + WriteOperand(Destination));
			}
			else
				into.AddInstructions("ALLOC_RSO NEXT " + WriteOperand(Destination), ResultType.Size);
		}

	}
}
