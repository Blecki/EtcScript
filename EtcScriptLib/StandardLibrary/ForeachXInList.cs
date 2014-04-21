using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void ForeachXInList(Environment Environment)
		{
			Environment.AddControl(Control.Create(
				Declaration.Parse("foreach (x) in (list)"),
				ControlBlockType.RequiredBlock,
				(parameters, body) =>
				{
					if (parameters[0] is Ast.Identifier)
					{
						return new ForeachIn(parameters[0].Source,
							(parameters[0] as Ast.Identifier).Name.Value,
							parameters[1],
							body);
					}
					else
						throw new CompileError("Expected identifier", parameters[0].Source);
				}));
		}

		private class ForeachIn : Ast.Statement
		{
			public String VariableName;
			public Ast.Node List;
			public Ast.Node Body;

			Variable TotalVariable;
			Variable CounterVariable;
			Variable ListVariable;
			Variable ValueVariable;

			public ForeachIn(Token Source, String VariableName, Ast.Node List, Ast.Node Body)
				: base(Source)
			{
				this.VariableName = VariableName;
				this.List = List;
				this.Body = Body;
			}

			public override Ast.Node Transform(ParseScope Scope)
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

			public override void Emit(VirtualMachine.InstructionList into, Ast.OperationDestination Destination)
			{
				List.Emit(into, Ast.OperationDestination.Stack);
				into.AddInstructions("LENGTH PEEK PUSH", "MOVE NEXT PUSH", 0);
				var LoopStart = into.Count;

				into.AddInstructions(
					"LOAD_PARAMETER NEXT R", TotalVariable.Offset,
					"GREATER_EQUAL PEEK R R",
					"IF_TRUE R",
					"JUMP NEXT", 0);

				var BreakPoint = into.Count - 1;

				into.AddInstructions("LOAD_PARAMETER NEXT R", ListVariable.Offset, "INDEX PEEK R PUSH");
				Body.Emit(into, Ast.OperationDestination.Discard);

				into.AddInstructions(
					"MOVE POP",
					"INCREMENT PEEK PEEK",
					"JUMP NEXT", LoopStart);

				into[BreakPoint] = into.Count;

				into.AddInstructions("CLEANUP NEXT", 3);
			}
		}
	}
}
