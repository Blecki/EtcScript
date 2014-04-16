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

		Variable TotalVariable;
		Variable CounterVariable;
		Variable ListVariable;
		Variable ValueVariable;

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
			var nestedScope = Scope.Push(ScopeType.Block);
			ListVariable = nestedScope.NewLocal("__list@" + VariableName, Scope.FindType("LIST"));
			TotalVariable = nestedScope.NewLocal("__total@" + VariableName, Scope.FindType("NUMBER"));
			CounterVariable = nestedScope.NewLocal("__counter@" + VariableName, Scope.FindType("number"));
			ValueVariable = nestedScope.NewLocal(VariableName, Scope.FindType("number"));
			Body = Body.Transform(nestedScope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			List.Emit(into, OperationDestination.Stack);
			into.AddInstructions("LENGTH PEEK PUSH", "MOVE NEXT PUSH", 0);
			var LoopStart = into.Count;

			into.AddInstructions(
				"LOAD_PARAMETER NEXT R", TotalVariable.Offset,
				"GREATER_EQUAL PEEK R R",
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions("LOAD_PARAMETER NEXT R", ListVariable.Offset, "INDEX PEEK R PUSH");
			Body.Emit(into, OperationDestination.Discard);

			into.AddInstructions(
				"MOVE POP",
				"INCREMENT PEEK PEEK",
				"JUMP NEXT", LoopStart);

			into[BreakPoint] = into.Count;

			into.AddInstructions("CLEANUP NEXT", 3);
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
