using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class DynamicMemberAccess : Node
	{
		public Node Object;
		public String Name;
		public Node DefaultValue;

		public DynamicMemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Generic;
			Object = Object.Transform(Scope);
			DefaultValue = DefaultValue.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("DYN_LOOKUP_MEMBER STRING R PUSH", into.AddString(Name));

			into.AddInstructions(
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var jumpSource = into.Count - 1;
			DefaultValue.Emit(into, OperationDestination.Top);
			into[jumpSource] = into.Count;

			if (Destination != OperationDestination.Stack)
				into.AddInstructions("MOVE POP " + Node.WriteOperand(Destination));
		}
	}
}
