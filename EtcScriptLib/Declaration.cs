using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum DeclarationType
	{
		Macro,
		Lambda,
		Test,
		Rule,
		System
	}

	public class Declaration
	{
		public DeclarationType Type;
		public List<DeclarationTerm> Terms;
		public LambdaBlock Body;
		public WhenClause WhenClause;
		internal ParseScope DeclarationScope;
		internal int OwnerContextID = 0;
		public Type ReturnType = EtcScriptLib.Type.Void;
		public String ReturnTypeName;
		public Object Tag;

		private List<Tuple<String, String>> HiddenArguments;

		public void AddHiddenArgument(String Name, String Type)
		{
			if (HiddenArguments == null) HiddenArguments = new List<Tuple<string, string>>();
			HiddenArguments.Add(new Tuple<string, string>(Name.ToUpper(), Type.ToUpper()));
		}

		public int ActualParameterCount
		{
			get
			{
				var termCount = Terms.Count(t => t.Type == DeclarationTermType.Term);
				if (HiddenArguments != null) termCount += HiddenArguments.Count;
				if (Type == DeclarationType.Lambda) return termCount + 1;
				else return termCount;
			}
		}

		public String DescriptiveHeader
		{
			get
			{
				return String.Join(" ", Terms.Select(term => term.ToString())) + " : " + ReturnTypeName;
			}
		}

		public VirtualMachine.InvokeableFunction MakeInvokableFunction()
		{
			return Body.GetBasicInvokable(ActualParameterCount);
		}

		public VirtualMachine.InvokeableFunction MakeWhenInvokable()
		{
			return WhenClause.GetBasicInvokable(ActualParameterCount);
		}

		public static Declaration Parse(String Header)
		{
			var tokenIterator = new TokenStream(new Compile.StringIterator(Header), new ParseContext());
			var headerTerms = EtcScriptLib.Parse.ParseMacroDeclarationHeader(tokenIterator,
				EtcScriptLib.Parse.DeclarationHeaderTerminatorType.StreamEnd);

			var r = new Declaration();
			r.ReturnTypeName = "VOID";

			if (!tokenIterator.AtEnd() && tokenIterator.Next().Type == TokenType.Colon)
			{
				tokenIterator.Advance();
				if (tokenIterator.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", tokenIterator);
				r.ReturnTypeName = tokenIterator.Next().Value.ToUpper();
				tokenIterator.Advance();
			}

			if (!tokenIterator.AtEnd()) throw new CompileError("Header did not end when expected");

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
					else if (currentTerm.Type == DeclarationTermType.Term)
						r.Add(new Ast.Literal(new Token(), null, currentTerm.DeclaredTypeName));
				}
				/*else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.NoneOrMany ||
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
				}*/
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
				/*else if (currentTerm.RepetitionType == DeclarationTermRepetitionType.NoneOrMany)
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
				 * */
			}

			if (nodeIndex < Nodes.Count) return false;
			return true;
		}

		public void ResolveTypes(ParseScope Scope)
		{
			try
			{
				DeclarationScope = Scope.Push(ScopeType.Function);
				DeclarationScope.Owner = this;

				if (!String.IsNullOrEmpty(ReturnTypeName))
				{
					ReturnType = Scope.FindType(ReturnTypeName);
					if (ReturnType == null)
					{
						if (Body != null && Body.Body != null)
							throw new CompileError("Could not find type '" +
								ReturnTypeName + "'.", Body.Body.Source);
						else
							throw new CompileError("Could not find type '" +
								ReturnTypeName + "'.");
					}
				}
				else
					ReturnType = EtcScriptLib.Type.Void;

				CreateParameterDescriptors();

				var header = DescriptiveHeader;
				Body.Name = header;
				if (WhenClause != null) WhenClause.Name = header + " : when ...";
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + " while resolving types on " + DescriptiveHeader + "\n" + e.StackTrace);
			}
		}

		// Given a list of terms, create the variable objects to represent the parameters of the declaration
		private void CreateParameterDescriptors()
		{
			int parameterIndex = -3;
			if (HiddenArguments != null)
			{
				for (int i = HiddenArguments.Count - 1; i >= 0; --i)
				{
					if (Type != DeclarationType.System)
					{
						var hiddenArgument = new Variable();
						hiddenArgument.StorageMethod = VariableStorageMethod.Local;
						hiddenArgument.Name = HiddenArguments[i].Item1;
						hiddenArgument.DeclaredTypeName = HiddenArguments[i].Item2;
						hiddenArgument.DeclaredType = DeclarationScope.FindType(hiddenArgument.DeclaredTypeName);
						if (hiddenArgument.DeclaredType == null) throw new CompileError("Could not find type for hidden argument '"
							+ hiddenArgument.DeclaredTypeName + "'.");
						hiddenArgument.Offset = parameterIndex;
						DeclarationScope.Variables.Add(hiddenArgument);
					}

					--parameterIndex;
				}
			}

			for (int i = Terms.Count - 1; i >= 0; --i)
			{
				if (Terms[i].Type == DeclarationTermType.Term)
				{
					var declaredType = String.IsNullOrEmpty(Terms[i].DeclaredTypeName) ?
						EtcScriptLib.Type.Generic : DeclarationScope.FindType(Terms[i].DeclaredTypeName);
					if (declaredType == null) throw new CompileError("Could not find type '" + Terms[i].DeclaredTypeName + "'.");
					Terms[i].DeclaredType = declaredType;

					if (Type != DeclarationType.System)
					{
						var variable = new Variable
						{
							Name = Terms[i].Name,
							Offset = parameterIndex,
							DeclaredType = declaredType
						};
						DeclarationScope.Variables.Add(variable);
					}

					--parameterIndex;
				}
			}
		}

		public void Transform(int OwnerContextID)
		{
			//this.OwnerContextID = OwnerContextID;
			Body.Transform(DeclarationScope);
			if (WhenClause != null) WhenClause.Transform(DeclarationScope);
		}

		public void Emit(VirtualMachine.InstructionList Into)
		{
			Body.EmitInstructions(DeclarationScope, Into);
			if (WhenClause != null) WhenClause.EmitInstructions(DeclarationScope, Into);
			foreach (var lambdaBody in DeclarationScope.ChildLambdas)
				lambdaBody.Emit(Into);
		}

		public void ResolveCallPoints()
		{
			Body.ResolveCallPoints();
			if (WhenClause != null) WhenClause.ResolveCallPoints();
			foreach (var lambdaBody in DeclarationScope.ChildLambdas)
				lambdaBody.ResolveCallPoints();
		}
	}
}
