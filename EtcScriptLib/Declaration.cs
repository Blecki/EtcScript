using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class Declaration
	{
		public String UsageSpecifier;
		public List<DeclarationTerm> Terms;
		public LambdaBlock Body;
		public WhenClause WhenClause;
		internal ParseScope DeclarationScope;
		internal int OwnerContextID = 0;
		public Type ReturnType = Type.Void;
		public String ReturnTypeName;

		public VirtualMachine.InvokeableFunction MakeInvokableFunction()
		{
			return Body.GetInvokable(Terms);
		}

		public VirtualMachine.InvokeableFunction MakeWhenInvokable()
		{
			return WhenClause.GetInvokable(Terms);
		}

		public static Declaration Parse(String Header)
		{
			var tokenIterator = new TokenStream(new Compile.StringIterator(Header), new ParseContext());
			var headerTerms = EtcScriptLib.Parse.ParseMacroDeclarationHeader(tokenIterator, 
				EtcScriptLib.Parse.DeclarationHeaderTerminatorType.StreamEnd);

			var r = new Declaration();
			r.Terms = headerTerms;
			r.Body = new LambdaBlock(null);
			return r;
		}

		public static bool AreTermsCompatible(List<DeclarationTerm> A, List<DeclarationTerm> B)
		{
			if (A.Count != B.Count) return false;
			for (int i = 0; i < A.Count; ++i)
			{
				if (A[i].Type != B[i].Type) return false;
				if (A[i].Type == DeclarationTermType.Keyword && A[i].Name != B[i].Name) return false;
				if (A[i].RepetitionType != B[i].RepetitionType) return false;
			}
			return true;
		}

		public static bool AreTermTypesCompatible(List<DeclarationTerm> A, List<DeclarationTerm> B)
		{
			if (A.Count != B.Count) return false;
			for (int i = 0; i < A.Count; ++i)
			{
				if (A[i].Type == DeclarationTermType.Term &&
					A[i].DeclaredTypeName != B[i].DeclaredTypeName) return false;
			}
			return true;
		}

		public static Ast.AssembleList GenerateParameterListSyntaxTree(List<Ast.Node> Nodes, List<DeclarationTerm> Header)
		{
			var r = new List<Ast.Node>();

			var termIndex = 0;
			var nodeIndex = 0;

			while (termIndex < Header.Count)
			{
				var currentTerm = Header[termIndex];

				bool match = false;
				if (nodeIndex < Nodes.Count) match = DeclarationTerm.Matches(currentTerm, Nodes[nodeIndex]);

				if (currentTerm.RepetitionType == DeclarationTermRepetitionType.Once)
				{
					if (currentTerm.Type == DeclarationTermType.Term) r.Add(Nodes[nodeIndex]);
					termIndex += 1;
					nodeIndex += 1;
				}
				else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.Optional)
				{
					termIndex += 1;
					if (match)
					{
						if (currentTerm.Type == DeclarationTermType.Term) r.Add(Nodes[nodeIndex]);
						nodeIndex += 1;
					}
					else
						if (currentTerm.Type == DeclarationTermType.Term) r.Add(new Ast.Literal(new Token(), null));
				}
				else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.NoneOrMany ||
					currentTerm.RepetitionType == DeclarationTermRepetitionType.OneOrMany)
				{
					var localList = new List<Ast.Node>();
					termIndex += 1;
					while (match)
					{
						if (currentTerm.Type == DeclarationTermType.Term) localList.Add(Nodes[nodeIndex]);
						nodeIndex += 1;
						match = (nodeIndex < Nodes.Count) ? DeclarationTerm.Matches(currentTerm, Nodes[nodeIndex]) : false;
					}
					if (currentTerm.Type == DeclarationTermType.Term) r.Add(new Ast.AssembleList(new Token(), localList));
				}
			}

			return new Ast.AssembleList(new Token(), r);
		}

		public static bool MatchesHeaderPattern(List<Ast.Node> Nodes, List<DeclarationTerm> Header)
		{
			var termIndex = 0;
			var nodeIndex = 0;

			while (termIndex < Header.Count)
			{
				var currentTerm = Header[termIndex];

				bool match = false;
				if (nodeIndex < Nodes.Count) match = DeclarationTerm.Matches(currentTerm, Nodes[nodeIndex]);

				if (currentTerm.RepetitionType == DeclarationTermRepetitionType.Once)
				{
					if (!match) return false;
					termIndex += 1;
					nodeIndex += 1;
				}
				else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.Optional)
				{
					termIndex += 1;
					if (match) nodeIndex += 1;
				}
				else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.NoneOrMany)
				{
					termIndex += 1;
					while (match)
					{
						nodeIndex += 1;
						match = (nodeIndex < Nodes.Count) ? DeclarationTerm.Matches(currentTerm, Nodes[nodeIndex]) : false;
					}
				}
				else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.OneOrMany)
				{
					termIndex += 1;
					if (!match) return false;
					while (match)
					{
						nodeIndex += 1;
						match = (nodeIndex < Nodes.Count) ? DeclarationTerm.Matches(currentTerm, Nodes[nodeIndex]) : false;
					}
				}
			}

			if (nodeIndex < Nodes.Count) return false;
			return true;
		}

	}
}
