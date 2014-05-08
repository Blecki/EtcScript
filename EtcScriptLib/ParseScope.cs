using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum ScopeType
	{
		Root,
		Block,
		Function
	}

	public class CapturedVariable
	{
		public Variable Source;
		public Variable LocalCopy;
	}

	public class ParseScope
	{
		public ParseScope Parent = null;
		public List<Declaration> Macros = new List<Declaration>();
		public List<Variable> Variables = new List<Variable>();
		public List<Type> Types = new List<Type>();
		public ParseContext EnvironmentContext = null;
		public int VariableIndex = 0;
		public ScopeType Type { get; private set; }
		public List<int> ReturnJumpSources;
		public Declaration Owner;
		public List<CapturedVariable> CapturedVariables = new List<CapturedVariable>();
		public List<Declaration> ChildLambdas = new List<Declaration>();

		public ParseScope(ScopeType Type)
		{
			this.Type = Type;
			if (Type == ScopeType.Function)
				ReturnJumpSources = new List<int>();
		}
	
		public Declaration FindMacro(List<Ast.Node> Nodes)
		{
			var r = Macros.FirstOrDefault((declaration) =>
				{
					return Declaration.MatchesHeaderPattern(Nodes, declaration.Terms);
				});
			if (r == null && Parent != null) return Parent.FindMacro(Nodes);
			return r;
		}

		public Variable FindVariable(String Name)
		{
			var r = Variables.FirstOrDefault(variable => variable.Name == Name);
			if (r != null) return r; //Found a match; stop searching.
			if (Parent == null) return r; //No parent; stop searching.

			r = Parent.FindVariable(Name);
			if (r == null) return r; //No match on parent either; stop searching.
			if (this.Type != ScopeType.Function) return r; //If we aren't searching a lambda, there's
			//no need to capture.

			if (r.StorageMethod == VariableStorageMethod.Member)
			{
				//Since code-sourcing constructs like Macros and Lambdas cannot appear within a type, 
				//	they should never be able to encounter a member variable through the scoping
				//	mechanism - so assume any member variable matched is an error.
				throw new InvalidProgramException();
			}

			if (
				r.StorageMethod == VariableStorageMethod.LambdaCapture ||
				r.StorageMethod == VariableStorageMethod.Local)
			{
				//If the variable is a local, or was captured by a higher order lambda, it needs to be
				//	captured.
				var capturedVariable = new CapturedVariable
				{
					Source = r,
					LocalCopy = new Variable
					{
						Name = r.Name,
						DeclaredTypeName = r.DeclaredTypeName,
						DeclaredType = r.DeclaredType,
						StorageMethod = VariableStorageMethod.LambdaCapture,
						Offset = CapturedVariables.Count
					}
				};

				CapturedVariables.Add(capturedVariable);

				return capturedVariable.LocalCopy;
			}
		
			return r;
		}

		public Type FindType(String Name)
		{
			var r = Types.FirstOrDefault(type => type.Name == Name);
			if (r == null && Parent != null) return Parent.FindType(Name);
			return r;
		}

		public Variable NewLocal(String Name, Type Type)
		{
			var r = new Variable { Name = Name.ToUpper(), Offset = VariableIndex, DeclaredType = Type };
			VariableIndex += 1;
			Variables.Add(r);
			return r;
		}

		public ParseScope Push(ScopeType Type)
		{
			var r = new ParseScope(Type);
			r.Parent = this;
			r.VariableIndex = (Type == ScopeType.Function ? 0 : this.VariableIndex);
			r.EnvironmentContext = this.EnvironmentContext;
			return r;
		}

		public void RecordReturnJumpSource(int Source)
		{
			if (Type == ScopeType.Function)
			{
				if (ReturnJumpSources == null) throw new InvalidProgramException();
				ReturnJumpSources.Add(Source);
			}
			else
			{
				if (Parent == null) throw new InvalidProgramException();
				Parent.RecordReturnJumpSource(Source);
			}
		}

		public Type OwnerFunctionReturnType
		{
			get
			{
				return OwnerFunction.ReturnType;
			}
		}

		public Declaration OwnerFunction
		{
			get
			{
				if (Type == ScopeType.Function)
				{
					if (Owner == null) throw new InvalidProgramException();
					return Owner;
				}
				else
				{
					if (Parent == null) throw new InvalidProgramException();
					return Parent.OwnerFunction;
				}
			}
		}

		internal List<Declaration> FindAllPossibleMacroMatches(List<Ast.Node> Arguments)
		{
			var r = new List<Declaration>(Macros.Where(declaration => Declaration.MatchesHeaderPattern(Arguments, declaration.Terms)));
			if (Parent != null) r.AddRange(Parent.FindAllPossibleMacroMatches(Arguments));
			return r;
		}

		internal void AddChildLambda(Declaration Function)
		{
			if (Type == ScopeType.Function)
			{
				if (ReturnJumpSources == null) throw new InvalidProgramException();
				ChildLambdas.Add(Function);
			}
			else
			{
				if (Parent == null) throw new InvalidProgramException();
				Parent.AddChildLambda(Function);
			}
		}
	}
}
