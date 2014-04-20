using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class CompatibleCall : Statement
	{
		public AssembleList Parameters;
		public String ResultTypename;

		public CompatibleCall(Token Source, List<Node> Parameters, String ResultTypename)
			: base(Source)
		{
			this.Parameters = new AssembleList(Source, Parameters);
			this.ResultTypename = ResultTypename;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType(ResultTypename);
			if (ResultType == null) throw new CompileError("Could not find type '" + ResultTypename + "'.", Source);
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
	}
}
