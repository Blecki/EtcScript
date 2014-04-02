using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class While : Statement
	{
		public Node Condition;
		public Node Body;

		public While(Token Source, Node Condition, Node Body) : base(Source) 
		{
			this.Condition = Condition;
			this.Body = Body;
		}

		public override Node Transform(ParseScope Scope)
		{
			Condition = Condition.Transform(Scope);
			Body = Body.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			var LoopStart = into.Count;
			
			Condition.Emit(into, OperationDestination.R);
			
			into.AddInstructions(
				"IF_FALSE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			Body.Emit(into, OperationDestination.Discard);

			into.AddInstructions("JUMP NEXT", LoopStart);

			into[BreakPoint] = into.Count;
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "While");
			Condition.Debug(depth + 1);
			Console.WriteLine(new String(' ', depth * 3) + "Do");
			Body.Debug(depth + 1);
		}
	}
}
