using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class StaticInvokation : Statement
	{
		public List<Node> Arguments;

		public StaticInvokation(Token Source, List<Node> Arguments) : base(Source) 
		{
			this.Arguments = Arguments;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException("Static invokation should have been removed by transformation phase");
		}

		/// <summary>
		/// Try to match an argument list's types with a declaration. As a side effect of the matching process,
		///		arguments are transformed. Return the transformed arguments so that they don't have to be
		///		transformed again.
		/// </summary>
		/// <param name="With"></param>
		/// <param name="Arguments"></param>
		/// <param name="Scope"></param>
		/// <returns>True and the transformed arguments if match was successful; false if unsuccessful.</returns>
		private Tuple<bool, List<Node>> TryTypeMatch(Declaration With, List<Node> Arguments, ParseScope Scope)
		{
			var transformedArguments = new List<Node>();
			int argumentIndex = 0;

			foreach (var term in With.Terms)
			{
				if (term.Type == DeclarationTermType.Keyword)
				{
					if (Arguments[argumentIndex] is Identifier)
					{
						if ((Arguments[argumentIndex] as Identifier).Name.Value.ToUpper() == term.Name)
						{
							++argumentIndex;
							continue;
						}
					}
					
					if (term.RepetitionType != DeclarationTermRepetitionType.Optional)
						return new Tuple<bool, List<Node>>(false, null);

					continue;
				}

				try {
					var transformedArgument = Arguments[argumentIndex].Transform(Scope);

					//Incompatible types here are not an error; they are a substitution failure.
					var compatibilityResult = Type.AreTypesCompatible(transformedArgument.ResultType, term.DeclaredType, Scope);
					if (!compatibilityResult.Compatible)
						return new Tuple<bool, List<Node>>(false, null);

					if (compatibilityResult.ConversionRequired)
						transformedArgument = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro,
							transformedArgument).Transform(Scope);
						
					transformedArguments.Add(transformedArgument);
					++argumentIndex;
				}
				catch (CompileError ce) //SFINAE!
				{
					//If transformation threw an error, then this macro is not a match.
					return new Tuple<bool, List<Node>>(false, null);
				}
			}

			//If we did not use every argument, this isn't a match.
			if (argumentIndex != Arguments.Count) return new Tuple<bool,List<Node>>(false, null);

			return new Tuple<bool,List<Node>>(true, transformedArguments);
		}

		private Tuple<Declaration, List<Node>> FindTypeMatch(List<Declaration> PossibleMatches, List<Node> Arguments, ParseScope Scope)
		{
			foreach (var possibleMatch in PossibleMatches)
			{
				var matches = TryTypeMatch(possibleMatch, Arguments, Scope);
				if (matches.Item1) return new Tuple<Declaration, List<Node>>(possibleMatch, matches.Item2);
			}

			//No macros matches
			return null;
		}

		public override Ast.Node Transform(ParseScope Scope)
		{
			//First, check the first argument and see if it can be transformed, and the result is a lambda.
			//		If so, transform into a compatible call of that lambda.


			var possibleMatches = Scope.FindAllPossibleMacroMatches(Arguments);
			var match = FindTypeMatch(possibleMatches, Arguments, Scope);
			if (match == null)
				throw new CompileError("Could not find matching function for static invokation", Source);

			ResultType = match.Item1.ReturnType;

			return CreateCorrectInvokationNode(Source, Scope, match.Item1, match.Item2).Transform(Scope);
		}

		public static Ast.Node CreateCorrectInvokationNode(
			Token Source, 
			ParseScope Scope, 
			Declaration Declaration, 
			List<Node> Arguments)
		{
			if (Declaration.OwnerContextID == Scope.EnvironmentContext.ID && Declaration.OwnerContextID != 0)
				return new Ast.JumpCall(Source, Declaration, Arguments);
			else
				return new Ast.FunctionCall(Source, Declaration.MakeInvokableFunction(), Arguments);
		}
	}
}
