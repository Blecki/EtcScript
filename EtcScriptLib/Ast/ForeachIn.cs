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
			ResultType = Type.Void;
			List = List.Transform(Scope);
			Body = Body.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			List.Emit(into, OperationDestination.R);
			into.AddInstructions(
				"SET_VARIABLE R STRING", into.AddString("__source@" + VariableName),
				"SET_VARIABLE NEXT STRING", 0, into.AddString("__counter@" + VariableName),
				"LENGTH R R",
				"SET_VARIABLE R STRING", into.AddString("__total@" + VariableName));

			var LoopStart = into.Count;

			into.AddInstructions(
				"LOOKUP STRING R", into.AddString("__total@" + VariableName),
				"LOOKUP STRING PUSH", into.AddString("__counter@" + VariableName),
				"GREATER_EQUAL POP R R",
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions(
				"LOOKUP STRING R", into.AddString("__counter@" + VariableName),
				"LOOKUP STRING PUSH", into.AddString("__source@" + VariableName),
				"INDEX R POP PUSH",
				"SET_VARIABLE POP NEXT", VariableName);

			Body.Emit(into, OperationDestination.Discard);

			into.AddInstructions(
				"LOOKUP STRING R", into.AddString("__counter@" + VariableName),
				"INCREMENT R R",
				"SET_VARIABLE R STRING", into.AddString("__counter@" + VariableName),
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
