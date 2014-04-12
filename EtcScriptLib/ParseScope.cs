using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum ScopeType
	{
		Block,
		Function
	}

	public class ParseScope
	{
		public ParseScope Parent = null;
		public List<Declaration> Macros = new List<Declaration>();
		public List<Variable> Variables = new List<Variable>();
		public List<Type> Types = new List<Type>();
		public ParseContext EnvironmentContext = null;
		public int VariableIndex = 0;
		public ScopeType Type = ScopeType.Block;
		public List<int> ReturnJumpSources;
	
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
			if (r == null && Parent != null) return Parent.FindVariable(Name);
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
			var r = new Variable { Name = Name, Offset = VariableIndex, DeclaredType = Type };
			VariableIndex += 1;
			Variables.Add(r);
			return r;
		}

		public ParseScope Push()
		{
			var r = new ParseScope();
			r.Parent = this;
			r.VariableIndex = this.VariableIndex;
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

		internal void ChangeToFunctionType()
		{
			Type = ScopeType.Function;
			ReturnJumpSources = new List<int>();
		}
	}
}
