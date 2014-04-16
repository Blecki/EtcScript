using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class ParseContext
	{
		public RuleSet Rules = new RuleSet();
		public List<Declaration> PendingEmission = new List<Declaration>();
		public int ID = 0;

		public void EmitDeclarations()
		{
			var into = new VirtualMachine.InstructionList();

			foreach (var type in ActiveScope.Types)
				type.ResolveTypes(ActiveScope);

			foreach (var variable in ActiveScope.Variables)
				variable.ResolveType(ActiveScope);

			foreach (var macro in ActiveScope.Macros)
				macro.ResolveTypes(ActiveScope);

			foreach (var declaration in PendingEmission)
				declaration.ResolveTypes(ActiveScope);

			foreach (var declaration in PendingEmission)
				declaration.Transform(ID);

			foreach (var declaration in PendingEmission)
			{
				declaration.Emit(into);
			
				if (Compile.Debug)
				{
					Compile.EmitDebugDump(declaration);
					Compile.DebugWrite("\n");
				}
			}

			foreach (var declaration in PendingEmission)
				declaration.ResolveCallPoints();

			if (Compile.Debug)
			{
				Compile.DebugWrite("\nCOMPILED CODE:\n");
				VirtualMachine.Debug.DumpOpcode(into.Data, Compile.DebugWrite, 1);
				Compile.DebugWrite(" STRING TABLE:\n");
				for (int i = 0; i < into.StringTable.Count; ++i)
					Compile.DebugWrite(" " + i.ToString() + ": " + into.StringTable[i] + "\n");
			}

			PendingEmission.Clear();
		}

		public struct Operator
		{
			public String token;
			public VirtualMachine.InstructionSet instruction;
		}

		public List<Control> Controls = new List<Control>();
		public ParseScope ActiveScope { get; private set; }
		public ParseScope TopScope { get; private set; }

		public void PushNewScope(ScopeType Type)
		{
			var newScope = new ParseScope(Type) { Parent = ActiveScope, EnvironmentContext = this };
			ActiveScope = newScope;
		}

		public void PopScope()
		{
			if (ActiveScope.Parent == null) throw new InvalidOperationException("Can't pop top scope");
			ActiveScope = ActiveScope.Parent;
		}

		public ParseContext()
		{
			ActiveScope = new ParseScope(ScopeType.Root) { EnvironmentContext = this };
			TopScope = ActiveScope;
		}

		public Dictionary<int, List<Operator>> precedence = new Dictionary<int, List<Operator>>();
		public List<String> operatorStrings = new List<String>();

		public void AddOperator(int precedence, String token, VirtualMachine.InstructionSet instruction)
		{
			if (!this.precedence.ContainsKey(precedence))
				this.precedence.Add(precedence, new List<Operator>());
			this.precedence[precedence].Add(new Operator { token = token, instruction = instruction });

			var i = 0;
			for (; i < operatorStrings.Count && operatorStrings[i].Length > token.Length; ++i)
				;
			operatorStrings.Insert(i, token);
		}

		public Operator? FindOperator(String token)
		{
			foreach (var p in precedence)
				foreach (var op in p.Value)
					if (op.token == token) return op;
			return null;
		}

		public int FindPrecedence(String token)
		{
			foreach (var p in precedence)
				foreach (var op in p.Value)
					if (op.token == token) return p.Key;
			return -1;
		}

		public Control FindControl(List<Ast.Node> nodes)
		{
			return Controls.FirstOrDefault((declaration) =>
				{
					return Declaration.MatchesHeaderPattern(nodes, declaration.DeclarationTerms);
				});
		}

		public void AddControl(Control Control)
		{
			Controls.Add(Control);
		}
	}
}
