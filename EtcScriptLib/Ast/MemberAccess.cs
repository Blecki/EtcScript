using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class MemberAccess : Node
	{
		public class DummyKeyword : Node
		{
			public String Name;
			public DummyKeyword(String Name) : base(new Token()) { this.Name = Name; }
		}

		public class DummyTerm : Node
		{
			public DummyTerm(Type Type) : base(new Token()) { this.ResultType = Type; }
		}

		public class DummyTermOfAnyType : Node
		{
			public DummyTermOfAnyType() : base(new Token()) { }
		}

		public static DummyKeyword Keyword(String Name) { return new DummyKeyword(Name); }
		public static DummyTerm Term(Type Type) { return new DummyTerm(Type); }
		public static DummyTermOfAnyType TermOfAnyType() { return new DummyTermOfAnyType(); }
		public static List<Node> DummyArguments(params Node[] Arguments) { return new List<Node>(Arguments); }
		public static bool ExactDummyMatch(List<DeclarationTerm> Terms, List<Node> Arguments)
		{
			if (Terms.Count != Arguments.Count) return false;
			for (int i = 0; i < Terms.Count; ++i)
			{
				if (Terms[i].Type == DeclarationTermType.Keyword)
				{
					var dummyKeyword = Arguments[i] as DummyKeyword;
					if (dummyKeyword == null || (dummyKeyword.Name != Terms[i].Name)) return false;
				}
				else if (Terms[i].Type == DeclarationTermType.Term)
				{
					var dummyTerm = Arguments[i] as DummyTerm;
					if (dummyTerm != null)
					{
						if (!System.Object.ReferenceEquals(dummyTerm.ResultType, Terms[i].DeclaredType))
							return false;
					}
					else if (Arguments[i] is DummyTermOfAnyType)
						return true;
					else
						return false;
				}
				else
					throw new InvalidOperationException();
			}
			return true;
		}

		public Node Object;
		public String Name;

		public MemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);

			if (IsAssignmentTarget)
			{
				if (Object.ResultType.Origin == TypeOrigin.System)
				{
					//Try to find an access macro for this type.
					var setterArguments = DummyArguments(Keyword("SET"), Keyword(Name), Keyword("ON"), Term(Object.ResultType),
						Keyword("TO"), TermOfAnyType());
					var matchingSetter = Scope.FindAllPossibleMacroMatches(setterArguments).Where(d =>
						ExactDummyMatch(d.Terms, setterArguments)).FirstOrDefault();
					if (matchingSetter != null)
						return new ExplicitSetter(Source, matchingSetter, Object).Transform(Scope);
					else
					{
						setterArguments = DummyArguments(Keyword("SET"), Term(Scope.FindType("STRING")), Keyword("ON"),
							Term(Object.ResultType), Keyword("TO"), TermOfAnyType());
						matchingSetter = Scope.FindAllPossibleMacroMatches(setterArguments).Where(d =>
							ExactDummyMatch(d.Terms, setterArguments)).FirstOrDefault();
						if (matchingSetter != null)
							return new GenericSetter(Source, matchingSetter, Name, Object).Transform(Scope);
						else
							throw new CompileError("No macro of the form SET " + Name + " ON " +
								Object.ResultType.Name + " TO VALUE found.", Source);
					}
				}
				else if (Object.ResultType.Origin == TypeOrigin.Script)
					return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
				else
					throw new CompileError("Can't assign to this target", Source);
			}
			else
			{
				if (Object.ResultType.Origin == TypeOrigin.System)
				{
					//Try to find an access macro for this type.
					var getterArguments = DummyArguments(Keyword("GET"), Keyword(Name), Keyword("FROM"), Term(Object.ResultType));
					var matchingGetter = Scope.FindAllPossibleMacroMatches(getterArguments).Where(d =>
						ExactDummyMatch(d.Terms, getterArguments)).FirstOrDefault();
					if (matchingGetter != null)
						return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingGetter,
							new List<Node>(new Node[] { Object })).Transform(Scope);
					else
					{
						getterArguments = DummyArguments(Keyword("GET"), Term(Scope.FindType("STRING")), Keyword("FROM"),
							Term(Object.ResultType));
						matchingGetter = Scope.FindAllPossibleMacroMatches(getterArguments).Where(d =>
							ExactDummyMatch(d.Terms, getterArguments)).FirstOrDefault();
						if (matchingGetter != null)
							return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingGetter,
								new List<Node>(new Node[] { new Ast.StringLiteral(Source, Name), Object })).Transform(Scope);
						else
							throw new CompileError("No macro of the form GET " + Name + " FROM " + Object.ResultType.Name + " found.", Source);
					}
				}
				else if (Object.ResultType.Origin == TypeOrigin.Script)
					return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
				else if (Object.ResultType.Origin == TypeOrigin.Primitive)
					throw new CompileError("Can't access members of primitives.", Source);
				else
					throw new CompileError("Impossible.", Source);
			}
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
	}
}
