using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ForeachIn : Statement
	{
		public String VariableName;
		public Node List;
		public Node Body;

		public ForeachIn(Token Source, String VariableName, Node List, Node Body) : base(Source) 
		{
			this.VariableName = VariableName;
			this.List = List;
			this.Body = Body;
		}

		public override Node Transform(ParseScope Scope)
		{
			List = List.Transform(Scope);
			Body = Body.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			List.Emit(into, OperationDestination.R);
			into.AddInstructions(
				"SET_VARIABLE R NEXT", "__source@" + VariableName,
				"SET_VARIABLE NEXT NEXT", 0, "__counter@" + VariableName,
				"LENGTH R R",
				"SET_VARIABLE R NEXT", "__total@" + VariableName);

			var LoopStart = into.Count;

			into.AddInstructions(
				"LOOKUP NEXT R", "__total@" + VariableName,
				"LOOKUP NEXT PUSH", "__counter@" + VariableName,
				"GREATER_EQUAL POP R R",
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions(
				"LOOKUP NEXT R", "__counter@" + VariableName,
				"LOOKUP NEXT PUSH", "__source@" + VariableName,
				"INDEX R POP PUSH",
				"SET_VARIABLE POP NEXT", VariableName);

			Body.Emit(into, OperationDestination.Discard);

			into.AddInstructions(
				"LOOKUP NEXT R", "__counter@" + VariableName,
				"INCREMENT R R",
				"SET_VARIABLE R NEXT", "__counter@" + VariableName,
				"JUMP NEXT", LoopStart);

			into[BreakPoint] = into.Count;
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Foreach " + VariableName + " in");
			List.Debug(depth + 1);
			Console.WriteLine(new String(' ', depth * 3) + "Do");
			Body.Debug(depth + 1);
		}
	}
}
