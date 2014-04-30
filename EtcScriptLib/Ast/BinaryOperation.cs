using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class BinaryOperator : Node
	{
		private static Dictionary<String, List<String>> RawOperators;
		public Node LHS;
		public Node RHS;
		public ParseContext.Operator Operator;

		public BinaryOperator(Token Source, ParseContext.Operator Operator,
			Node LHS, Node RHS) : base(Source) 
		{
			this.Operator = Operator;
			this.LHS = LHS;
			this.RHS = RHS;
		}

		private void PopulateRawOperators()
		{
			if (RawOperators != null) return;

			RawOperators = new Dictionary<string, List<string>>();
			RawOperators.Add("NUMBER", new List<String>(new String[] { "+", "-", "*", "/", "%", "|", "&", "<", ">", "<=", ">=" }));
			RawOperators.Add("BOOLEAN", new List<String>(new String[] { "||", "&&" }));
			RawOperators.Add("STRING", new List<String>(new String[] { "+" }));
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}

		public override Node Transform(ParseScope Scope)
		{
			LHS = LHS.Transform(Scope);
			RHS = RHS.Transform(Scope);

			//Generics behave dynamically with operators.
			if (Object.ReferenceEquals(LHS.ResultType, Type.Generic) || Object.ReferenceEquals(RHS.ResultType, Type.Generic))
				return new RawBinaryOperator(Source, Operator.instruction, LHS, RHS, Type.Generic);

			//Equality works with everything.
			if (Operator.token == "==" || Operator.token == "!=")
				return new RawBinaryOperator(Source, Operator.instruction, LHS, RHS, Type.Generic);

			if (Object.ReferenceEquals(LHS.ResultType, RHS.ResultType))
			{
				PopulateRawOperators();
				if (RawOperators.ContainsKey(LHS.ResultType.Name))
					if (RawOperators[LHS.ResultType.Name].Contains(Operator.token))
						return new RawBinaryOperator(Source, Operator.instruction, LHS, RHS, LHS.ResultType);
			}

			//Try to find an operator macro for these types.
			var operatorArguments = DummyArguments(Term(LHS.ResultType), Keyword(Operator.token), Term(RHS.ResultType));
			var matchingOperator = Scope.FindAllPossibleMacroMatches(operatorArguments).Where(d =>
					ExactDummyMatch(d.Terms, operatorArguments)).FirstOrDefault();
			if (matchingOperator != null)
			{
				return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingOperator,
					new List<Node>(new Node[] { LHS, RHS })).Transform(Scope);
			}
			else
				throw new CompileError("No operator macro of the form " + LHS.ResultType.Name + " " +
					Operator.token + " " + RHS.ResultType.Name + " found.", Source);

		}
	}
}
