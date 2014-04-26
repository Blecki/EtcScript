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
			public Ast.Node Indexer;

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

				//Try to find an access macro for this type.
				var getterArguments = DummyArguments(Keyword("GET"), Keyword("AT"), Term(Scope.FindType("NUMBER")),
					Keyword("FROM"), Term(List.ResultType));
				var indexerMacro = Scope.FindAllPossibleMacroMatches(getterArguments).Where(d =>
					ExactDummyMatch(d.Terms, getterArguments)).FirstOrDefault();
				if (indexerMacro == null)
					throw new CompileError("No macro of the form GET AT NUMBER FROM " +
						List.ResultType.Name + " found.", Source);

				var nestedScope = Scope.Push(ScopeType.Block);

				ListVariable = nestedScope.NewLocal("__list@" + VariableName, Scope.FindType("LIST"));
				TotalVariable = nestedScope.NewLocal("__total@" + VariableName, Scope.FindType("NUMBER"));
				CounterVariable = nestedScope.NewLocal("__counter@" + VariableName, Scope.FindType("NUMBER"));
				ValueVariable = nestedScope.NewLocal(VariableName, indexerMacro.ReturnType);

				Indexer = Ast.StaticInvokation.CreateCorrectInvokationNode(Source, nestedScope, indexerMacro,
					new List<Ast.Node>(new Ast.Node[] { 
						new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "__counter@"+VariableName }),
						new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "__list@"+VariableName })
					})).Transform(nestedScope);

				Body = Body.Transform(nestedScope);
				return this;
			}

			public override void Emit(VirtualMachine.InstructionList into, Ast.OperationDestination Destination)
			{
				//Prepare loop control variables
				List.Emit(into, Ast.OperationDestination.Stack);	//__list@
				into.AddInstructions("LENGTH PEEK PUSH");			//__total@
				into.AddInstructions("MOVE NEXT PUSH", 0);			//__counter@

				var LoopStart = into.Count;

				into.AddInstructions(
					"LOAD_PARAMETER NEXT R #" + TotalVariable.Name, TotalVariable.Offset,
					"GREATER_EQUAL PEEK R R",
					"IF_TRUE R",
					"JUMP NEXT", 0);

				var BreakPoint = into.Count - 1;

				Indexer.Emit(into, Ast.OperationDestination.Stack);
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
