using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void ForeachXFromMinToMax(ParseContext Context)
		{
			Context.AddControl(Control.Create(
					Declaration.Parse("foreach (x) from (low) to (high)"),
					ControlBlockType.RequiredBlock,
					(parameters, body) =>
					{
						if (parameters[0] is Ast.Identifier)
						{
							return new ForeachFrom(parameters[0].Source,
								(parameters[0] as Ast.Identifier).Name.Value,
								parameters[1],
								parameters[2],
								body);
						}
						else
							throw new CompileError("Expected identifier", parameters[0].Source);
					}));
		}

		private class ForeachFrom : Ast.Statement
		{
			public String VariableName;
			public Ast.Node Min;
			public Ast.Node Max;
			public Ast.Node Body;

			Variable TotalVariable;
			Variable CounterVariable;
			Variable ValueVariable;

			public ForeachFrom(Token Source, String VariableName, Ast.Node Min, Ast.Node Max, Ast.Node Body)
				: base(Source)
			{
				this.VariableName = VariableName;
				this.Min = Min;
				this.Max = Max;
				this.Body = Body;
			}

			public override Ast.Node Transform(ParseScope Scope)
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

			public override void Emit(VirtualMachine.InstructionList into, Ast.OperationDestination Destination)
			{
				Max.Emit(into, Ast.OperationDestination.Stack);
				Min.Emit(into, Ast.OperationDestination.Stack);

				var LoopStart = into.Count;

				into.AddInstructions(
					"LOAD_PARAMETER NEXT R", TotalVariable.Offset,
					"GREATER PEEK R R",
					"IF_TRUE R",
					"JUMP NEXT", 0);

				var BreakPoint = into.Count - 1;

				into.AddInstructions("MOVE PEEK PUSH");

				Body.Emit(into, Ast.OperationDestination.Discard);

				into.AddInstructions(
					"MOVE POP",
					"INCREMENT PEEK PEEK",
					"JUMP NEXT", LoopStart);

				into[BreakPoint] = into.Count;

				into.AddInstructions(
					"MOVE POP",
					"MOVE POP");
			}
		}
	}
}
