using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	internal class ConsiderRule : Node
	{
		Rulebook Rulebook;
		internal List<Node> Arguments;
		internal ParseScope DeclarationScope;

		internal ConsiderRule(
			EtcScriptLib.Token Source,
			EtcScriptLib.Ast.Node Arguments
			)
			: base(Source)
		{
			if (Arguments is StaticInvokation)
				this.Arguments = (Arguments as StaticInvokation).Arguments;
			else throw new CompileError("Argument to ConsiderRule must be static invokation", Source);
		}

		public override void Emit(VirtualMachine.InstructionList Instructions, EtcScriptLib.Ast.OperationDestination Destination)
		{
			//Try each rule in Rulebook until one of them returns 0.
			List<int> JumpToEndPositions = new List<int>();

			foreach (var arg in Arguments)
				arg.Emit(Instructions, OperationDestination.Stack);

			foreach (var Rule in Rulebook.Rules)
			{
				bool quickCall = (Rule.OwnerContextID == DeclarationScope.EnvironmentContext.ID && Rule.OwnerContextID != 0);
					
				int whenSkipPoint = 0;
				if (Rule.WhenClause != null)
				{
					EmitCallInstruction(Instructions, Rule, Rule.WhenClause, quickCall);
					Instructions.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
					whenSkipPoint = Instructions.Count - 1;
				}

				EmitCallInstruction(Instructions, Rule, Rule.Body, quickCall);
				Instructions.AddInstructions("EQUAL NEXT R R", 0, "IF_TRUE R", "JUMP NEXT", 0);
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
			var assembleList = Declaration.GenerateParameterListSyntaxTree(Arguments, Rulebook.DeclarationTerms);
			Arguments = new List<Node>(assembleList.Members.Select(n => n.Transform(Scope)));

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

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Consider");
			foreach (var arg in Arguments)
				arg.Debug(depth + 1);
		}
	}
}
