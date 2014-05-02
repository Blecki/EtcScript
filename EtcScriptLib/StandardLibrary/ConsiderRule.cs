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

				var arguments = Declaration.GenerateParameterListSyntaxTree(Arguments, rulebook.DeclarationTerms).Members;
				var boxedArguments = new List<Ast.Node>(arguments.Select(n =>
				{
					return new Ast.Box(n.Source, n).Transform(Scope);
				}));

				return Ast.StaticInvokation.CreateCorrectInvokationNode(Source, Scope, rulebook.ConsiderFunction, 
					boxedArguments)
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
					var skipPoints = new List<int>();

					var sourceTerms = new List<DeclarationTerm>(Rule.Terms.Where(t => t.Type == DeclarationTermType.Term));
					for (int i = parameterCount; i > 0; --i)
					{
						Instructions.AddInstructions("LOAD_PARAMETER NEXT R", (-i - 2));
						Instructions.AddInstructions("LOAD_RSO_M R NEXT R", 0);

						var argumentTypeID = sourceTerms[i - 1].DeclaredType.ID;
						Instructions.AddInstructions("IS_ANCESTOR_OF R NEXT R", argumentTypeID);
						Instructions.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
						skipPoints.Add(Instructions.Count - 1);
					}

					if (Rule.WhenClause != null)
					{
						DuplicateArguments(Instructions, parameterCount);
						Instructions.AddInstructions("CALL NEXT", 0);
						Rule.WhenClause.CallPoints.Add(Instructions.Count - 1);
						Instructions.AddInstructions("CLEANUP NEXT", parameterCount);

						Instructions.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
						skipPoints.Add(Instructions.Count - 1);
					}

					DuplicateArguments(Instructions, parameterCount); 
					Instructions.AddInstructions("CALL NEXT", 0);
					Rule.Body.CallPoints.Add(Instructions.Count - 1);
					Instructions.AddInstructions("CLEANUP NEXT", parameterCount);

					if (useFallThrough)
						Instructions.AddInstructions("EQUAL NEXT R R", 0, "IF_TRUE R");

					Instructions.AddInstructions("JUMP NEXT", 0);
					JumpToEndPositions.Add(Instructions.Count - 1);

					foreach (var i in skipPoints) Instructions[i] = Instructions.Count;
				}

				if (Rulebook.DefaultValue != null)
				{
					DuplicateArguments(Instructions, parameterCount); 
					Instructions.AddInstructions("CALL NEXT", 0);
					Rulebook.DefaultValue.Body.CallPoints.Add(Instructions.Count - 1);
					Instructions.AddInstructions("CLEANUP NEXT", parameterCount);
				}

				foreach (var spot in JumpToEndPositions) Instructions[spot] = Instructions.Count;

				if (Destination != Ast.OperationDestination.R && Destination != Ast.OperationDestination.Discard)
					Instructions.AddInstructions("MOVE R " + WriteOperand(Destination));
			}

			private static void DuplicateArguments(VirtualMachine.InstructionList Instructions, int parameterCount)
			{
				for (int i = parameterCount; i > 0; --i)
				{
					Instructions.AddInstructions("LOAD_PARAMETER NEXT PUSH", (-i - 2));
					Ast.Box.EmitUnbox(Instructions, Ast.OperationDestination.Top, Ast.OperationDestination.Top);
				}
			}

			public override EtcScriptLib.Ast.Node Transform(ParseScope Scope)
			{
				ResultType = Rulebook.ResultType;
				return this;
			}

		}
	}
}
