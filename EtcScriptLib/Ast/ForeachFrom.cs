using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ForeachFrom : Statement
	{
		public String VariableName;
		public int Min;
		public int Max;
		public Node Body;

		public override void Emit(VirtualMachine.InstructionList into)
		{
			into.AddInstructions(
				"SET_VARIABLE NEXT NEXT", Max, "__total@" + VariableName,
				"SET_VARIABLE NEXT NEXT", Min, "__counter@" + VariableName);

			var LoopStart = into.Count;

			into.AddInstructions(
				"LOOKUP NEXT R", "__total@" + VariableName,
				"LOOKUP NEXT PUSH", "__counter@" + VariableName,
				"GREATER POP R R",
				"IF_TRUE R",
				"JUMP", 0);

			var BreakPoint = into.Count - 1;

			into.AddInstructions(
				"LOOKUP NEXT R", "__counter@" + VariableName,
				"SET_VARIABLE R NEXT", VariableName);

			Body.Emit(into);

			into.AddInstructions(
				"LOOKUP NEXT R", "__counter@" + VariableName,
				"INCREMENT R R",
				"SET_VARIABLE R NEXT", "__counter@" + VariableName,
				"JUMP", LoopStart);

			into[BreakPoint] = into.Count;
		}
	}
}
