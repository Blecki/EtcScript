using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Let : Statement
	{
		public Node LHS;
		public Node Value;

		public Let(Token Source, Node Object, Node Value) : base(Source) 
		{
			this.LHS = Object;
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			LHS = LHS.Transform(Scope);
			Value = Value.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (LHS is MemberAccess)
			{
				(LHS as MemberAccess).Object.Emit(into, OperationDestination.R);
				Value.Emit(into, OperationDestination.Stack);
				into.AddInstructions("SET_MEMBER POP NEXT R", (LHS as MemberAccess).Name);
			}
			else if (LHS is Identifier)
			{
				Value.Emit(into, OperationDestination.R);
				into.AddInstructions("SET_VARIABLE R NEXT", (LHS as Identifier).Name.Value);
			}
			else
				throw new InvalidProgramException();
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Assign");
			Value.Debug(depth + 1);
			Console.WriteLine(new String(' ', depth * 3) + "To");
			LHS.Debug(depth + 1);
		}
	}
}
