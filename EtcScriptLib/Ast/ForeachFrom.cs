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

		public ForeachFrom(Token Source, String VariableName, Node Min, Node Max, Node Body) : base(Source) 
		{
			this.VariableName = VariableName;
			this.Min = Min;
			this.Max = Max;
			this.Body = Body;
		}

		public override Node Transform(ParseScope Scope)
		{
			Min = Min.Transform(Scope);
			Max = Max.Transform(Scope);
			Body = Body.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Max.Emit(into, OperationDestination.R);
			into.AddInstructions("SET_VARIABLE R NEXT", "__total@" + VariableName);
			Min.Emit(into, OperationDestination.R);
			into.AddInstructions("SET_VARIABLE R NEXT", "__counter@" + VariableName);

			var LoopStart = into.Count;

			into.AddInstructions(
				"LOOKUP NEXT R", "__total@" + VariableName,
				"LOOKUP NEXT PUSH", "__counter@" + VariableName,
				"GREATER POP R R",
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions(
				"LOOKUP NEXT R", "__counter@" + VariableName,
				"SET_VARIABLE R NEXT", VariableName);

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
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Foreach From " + Min + " to " + Max);
			Body.Debug(depth + 1);
		}
	}
}
