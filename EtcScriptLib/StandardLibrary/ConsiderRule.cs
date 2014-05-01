using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void ConsiderRule(Environment Environment)
		{
			Environment.AddControl(Control.Create(
				Declaration.Parse("consider (x)"),
				ControlBlockType.NoBlock,
				(parameters, body) =>
				{
					return new ConsiderRuleBookNode(parameters[0].Source, parameters[0]);
				}));
		}

		internal class ConsiderRuleBookNode : Ast.Node
		{
			internal List<Ast.Node> Arguments;

			internal ConsiderRuleBookNode(
				EtcScriptLib.Token Source,
				EtcScriptLib.Ast.Node Arguments
				)
				: base(Source)
			{
				if (Arguments is Ast.StaticInvokation)
					this.Arguments = (Arguments as Ast.StaticInvokation).Arguments;
				else throw new CompileError("Argument to ConsiderRule must be static invokation", Source);
			}

			public override EtcScriptLib.Ast.Node Transform(ParseScope Scope)
			{
				var rulebook = Scope.EnvironmentContext.Rules.FindMatchingRulebook(Arguments);
				if (rulebook == null)
					return null; //Rulebook does not exist... no, this isn't an error!

				return Ast.StaticInvokation.CreateCorrectInvokationNode(Source, Scope, rulebook.ConsiderFunction, 
					Declaration.GenerateParameterListSyntaxTree(Arguments, rulebook.DeclarationTerms).Members)
					.Transform(Scope);
			}
		}

		internal class ConsiderRuleBookFunctionNode : Ast.Node
		{
			Rulebook Rulebook;

			internal ConsiderRuleBookFunctionNode(
				EtcScriptLib.Token Source,
				Rulebook Rulebook
				)
				: base(Source)
			{
				this.Rulebook = Rulebook;
			}

			public override void Emit(VirtualMachine.InstructionList Instructions, 
				EtcScriptLib.Ast.OperationDestination Destination)
			{
				bool useFallThrough = Rulebook.ResultTypeName == "RULE-RESULT";

				//Try each rule in Rulebook until one of them returns not-null.
				List<int> JumpToEndPositions = new List<int>();
				var parameterCount = Rulebook.DeclarationTerms.Count(t => t.Type == DeclarationTermType.Term);

				foreach (var Rule in Rulebook.Rules)
				{
					int whenSkipPoint = 0;
					if (Rule.WhenClause != null)
					{
						for (int i = parameterCount; i > 0; --i)
							Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 2));
						Instructions.AddInstructions("CALL NEXT", 0);
						Rule.WhenClause.CallPoints.Add(Instructions.Count - 1);
						Instructions.AddInstructions("CLEANUP NEXT", parameterCount);

						Instructions.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
						whenSkipPoint = Instructions.Count - 1;
					}

					//Duplicate arguments onto top of stack.
					for (int i = parameterCount; i > 0; --i)
						Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 2));
					Instructions.AddInstructions("CALL NEXT", 0);
					Rule.Body.CallPoints.Add(Instructions.Count - 1);
					Instructions.AddInstructions("CLEANUP NEXT", parameterCount);

					if (useFallThrough)
						Instructions.AddInstructions("EQUAL NEXT R R", 0, "IF_TRUE R");

					Instructions.AddInstructions("JUMP NEXT", 0);
					JumpToEndPositions.Add(Instructions.Count - 1);

					if (Rule.WhenClause != null) Instructions[whenSkipPoint] = Instructions.Count;
				}

				if (Rulebook.DefaultValue != null)
				{
					//Duplicate arguments onto top of stack.
					for (int i = parameterCount; i > 0; --i)
						Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 2));
					Instructions.AddInstructions("CALL NEXT", 0);
					Rulebook.DefaultValue.Body.CallPoints.Add(Instructions.Count - 1);
					Instructions.AddInstructions("CLEANUP NEXT", parameterCount);
				}

				foreach (var spot in JumpToEndPositions) Instructions[spot] = Instructions.Count;

				if (Destination != Ast.OperationDestination.R && Destination != Ast.OperationDestination.Discard)
					Instructions.AddInstructions("MOVE R " + WriteOperand(Destination));
			}

			public override EtcScriptLib.Ast.Node Transform(ParseScope Scope)
			{
				ResultType = Rulebook.ResultType;
				return this;
			}

		}
	}
}
