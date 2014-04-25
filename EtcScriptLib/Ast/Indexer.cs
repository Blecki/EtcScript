using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Indexer : Node
	{
		public Node Object;
		public Node Index;

		public Indexer(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);
			Index = Index.Transform(Scope);

			if (IsAssignmentTarget)
			{
				//Try to find an indexer macro for this type.
				var setterArguments = DummyArguments(Keyword("SET"), Keyword("AT"), Term(Index.ResultType),
					Keyword("ON"), Term(Object.ResultType), Keyword("TO"), TermOfAnyType());
				var matchingSetter = Scope.FindAllPossibleMacroMatches(setterArguments).Where(d =>
					ExactDummyMatch(d.Terms, setterArguments)).FirstOrDefault();
				if (matchingSetter != null)
					return new ExplicitIndexSetter(Source, matchingSetter, Object, Index).Transform(Scope);
				else
					throw new CompileError("No macro of the form SET AT " + Index.ResultType.Name + " ON " +
							Object.ResultType.Name + " TO VALUE found.", Source);
			}
			else
			{
				//Try to find an access macro for this type.
				var getterArguments = DummyArguments(Keyword("GET"), Keyword("AT"), Term(Index.ResultType),
					Keyword("FROM"), Term(Object.ResultType));
				var matchingGetter = Scope.FindAllPossibleMacroMatches(getterArguments).Where(d =>
					ExactDummyMatch(d.Terms, getterArguments)).FirstOrDefault();
				if (matchingGetter != null)
					return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingGetter,
						new List<Node>(new Node[] { Index, Object })).Transform(Scope);
				else
					throw new CompileError("No macro of the form GET AT " + Index.ResultType.Name + " FROM " + 
						Object.ResultType.Name + " found.", Source);
			}
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
	}
}
