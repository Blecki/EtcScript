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
					return new ConsiderRuleNode(parameters[0].Source, parameters[0]);
				}));
		}

		internal class ConsiderRuleNode : Ast.Node
		{
			Rulebook Rulebook;
			internal List<Ast.Node> Arguments;
			internal ParseScope DeclarationScope;

			internal ConsiderRuleNode(
				EtcScriptLib.Token Source,
				EtcScriptLib.Ast.Node Arguments
				)
				: base(Source)
			{
				if (Arguments is Ast.StaticInvokation)
					this.Arguments = (Arguments as Ast.StaticInvokation).Arguments;
				else throw new CompileError("Argument to ConsiderRule must be static invokation", Source);
			}

			public override void Emit(VirtualMachine.InstructionList Instructions, 
				EtcScriptLib.Ast.OperationDestination Destination)
			{
				bool useFallThrough = Rulebook.ResultTypeName == "RULE-RESULT";

				//Try each rule in Rulebook until one of them returns not-null.
				List<int> JumpToEndPositions = new List<int>();

				foreach (var arg in Arguments)
					arg.Emit(Instructions, Ast.OperationDestination.Stack);

				foreach (var Rule in Rulebook.Rules)
				{
					bool quickCall =
						Rule.OwnerContextID == DeclarationScope.EnvironmentContext.ID
						&& Rule.OwnerContextID != 0
						&& DeclarationScope.OwnerFunction.OwnerContextID != 0;

					int whenSkipPoint = 0;
					if (Rule.WhenClause != null)
					{
						if (quickCall)
						{
							Instructions.AddInstructions("CALL NEXT", 0);
							Rule.WhenClause.CallPoints.Add(Instructions.Count - 1);
						}
						else
						{
							//Duplicate arguments onto top of stack.
							Instructions.AddInstructions("MOVE F PUSH", "MARK_STACK F");
							for (int i = Arguments.Count; i > 0; --i)
								Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 1));
							Instructions.AddInstructions("MOVE NEXT R",
								Rule.WhenClause.GetBasicInvokable(Rule.ActualParameterCount),
								"STACK_INVOKE R",
								"CLEANUP NEXT", Arguments.Count,
								"MOVE POP F");
						}

						Instructions.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
						whenSkipPoint = Instructions.Count - 1;
					}

					if (quickCall)
					{
						Instructions.AddInstructions("CALL NEXT", 0);
						Rule.Body.CallPoints.Add(Instructions.Count - 1);
					}
					else
					{
						//Duplicate arguments onto top of stack.
						Instructions.AddInstructions("MOVE F PUSH", "MARK_STACK F");
						for (int i = Arguments.Count; i > 0; --i)
							Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 1));
						Instructions.AddInstructions("MOVE NEXT R",
							Rule.Body.GetBasicInvokable(Rule.ActualParameterCount),
							"STACK_INVOKE R",
							"CLEANUP NEXT", Arguments.Count,
							"MOVE POP F");
					}

					if (useFallThrough)
						Instructions.AddInstructions("EQUAL NEXT R R", 0, "IF_TRUE R");

					Instructions.AddInstructions("JUMP NEXT", 0);
					JumpToEndPositions.Add(Instructions.Count - 1);

					if (Rule.WhenClause != null) Instructions[whenSkipPoint] = Instructions.Count;
				}

				foreach (var spot in JumpToEndPositions) Instructions[spot] = Instructions.Count;

				if (Arguments.Count > 0) Instructions.AddInstructions("CLEANUP NEXT", Arguments.Count);
			}

			private static void EmitCallInstruction(
				VirtualMachine.InstructionList Instructions,
				Declaration Rule,
				LambdaBlock Block,
				bool quickCall)
			{
				if (quickCall)
				{
					Instructions.AddInstructions("MOVE NEXT R", 0, "CALL R");
					Block.CallPoints.Add(Instructions.Count - 2);
				}
				else
				{
					Instructions.AddInstructions("MOVE NEXT R", Block.GetBasicInvokable(Rule.ActualParameterCount), "STACK_INVOKE R");
				}
			}

			public override EtcScriptLib.Ast.Node Transform(ParseScope Scope)
			{
				ResultType = Type.Void;
				DeclarationScope = Scope;

				//All the rules in the rulebook should be determined by now.
				Rulebook = Scope.EnvironmentContext.Rules.FindMatchingRulebook(Arguments);
				if (Rulebook == null)
					return null;

				ResultType = Rulebook.ResultType;
				
				var assembleList = Declaration.GenerateParameterListSyntaxTree(Arguments, Rulebook.DeclarationTerms);
				Arguments = new List<Ast.Node>(assembleList.Members.Select(n => n.Transform(Scope)));


				//Check types
				int argumentIndex = 0;
				foreach (var term in Rulebook.DeclarationTerms)
					if (term.Type == DeclarationTermType.Term)
					{
						var compatibilityResult = Type.AreTypesCompatible(Arguments[argumentIndex].ResultType, term.DeclaredType, Scope);
						if (!compatibilityResult.Compatible)
							Type.ThrowConversionError(Arguments[argumentIndex].ResultType, term.DeclaredType, Source);

						if (compatibilityResult.ConversionRequired)
							Arguments[argumentIndex] = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro,
								Arguments[argumentIndex]).Transform(Scope);

						argumentIndex += 1;
					}

				return this;
			}

		}
	}
}
