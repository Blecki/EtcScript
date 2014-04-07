using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Return : Statement
	{
		public Node Value;

		public Return(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			if (Value != null) Value = Value.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Value != null)
			{
				Value.Emit(into, OperationDestination.R);
			}

			into.AddInstructions(
				"LOOKUP STRING PUSH", into.AddString("@stack-size"),
				"RESTORE_STACK POP",
				"CONTINUE POP");
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Return");
			if (Value != null) Value.Debug(depth + 1);
		}
	}
}
