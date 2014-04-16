using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ForeachFrom : Statement
	{
		public String VariableName;
		public Node Min;
		public Node Max;
		public Node Body;

		Variable TotalVariable;
		Variable CounterVariable;
		Variable ValueVariable;

		public ForeachFrom(Token Source, String VariableName, Node Min, Node Max, Node Body) : base(Source) 
		{
			this.VariableName = VariableName;
			this.Min = Min;
			this.Max = Max;
			this.Body = Body;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;
			Max = Max.Transform(Scope);
			var nestedScope = Scope.Push(ScopeType.Block);
			TotalVariable = nestedScope.NewLocal("__total@" + VariableName, Scope.FindType("NUMBER"));
			Min = Min.Transform(nestedScope);
			CounterVariable = nestedScope.NewLocal("__counter@" + VariableName, Scope.FindType("NUMBER"));
			ValueVariable = nestedScope.NewLocal(VariableName, Scope.FindType("NUMBER"));
			Body = Body.Transform(nestedScope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Max.Emit(into, OperationDestination.Stack);
			Min.Emit(into, OperationDestination.Stack);

			var LoopStart = into.Count;

			into.AddInstructions(
				"LOAD_PARAMETER NEXT R", TotalVariable.Offset,
				"GREATER PEEK R R",
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions("MOVE PEEK PUSH");

			Body.Emit(into, OperationDestination.Discard);

			into.AddInstructions(
				"MOVE POP",
				"INCREMENT PEEK PEEK",
				"JUMP NEXT", LoopStart);

			into[BreakPoint] = into.Count;

			into.AddInstructions(
				"MOVE POP",
				"MOVE POP");
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Foreach From " + Min + " to " + Max);
			Body.Debug(depth + 1);
		}
	}
}
