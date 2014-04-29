using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class CompatibleCall : Statement
	{
		public List<Node> Parameters;
		public String ResultTypename;

		public CompatibleCall(Token Source, List<Node> Parameters, String ResultTypename)
			: base(Source)
		{
			this.Parameters = Parameters;
			this.ResultTypename = ResultTypename;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType(ResultTypename);
			if (ResultType == null) throw new CompileError("Could not find type '" + ResultTypename + "'.", Source);
			Parameters = new List<Node>(Parameters.Select(n => n.Transform(Scope)));
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			foreach (var n in Parameters)
				n.Emit(into, OperationDestination.Stack);
			into.AddInstructions("COMPAT_INVOKE NEXT", Parameters.Count);
			if (Destination != OperationDestination.R && Destination != OperationDestination.Discard)
				into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
		}
	}
}
