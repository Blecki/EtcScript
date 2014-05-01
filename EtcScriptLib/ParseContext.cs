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
		public List<Ast.Node> Initialization = new List<Ast.Node>();
		public Declaration InitializationFunction;
		public int ID = 0;
		public Func<String, Object, LoadedFile> FileLoader;

		public void EmitDeclarations(int StaticVariableOffset)
		{
			var into = new VirtualMachine.InstructionList();

			foreach (var type in TopScope.Types)
				type.ResolveTypes(TopScope);

			foreach (var type in TopScope.Types)
				type.AssignMemberOffsets();

			int variableOffset = StaticVariableOffset;
			foreach (var variable in TopScope.Variables)
			{
				//System.Diagnostics.Debug.Assert(variable.StorageMethod == VariableStorageMethod.Static);
				variable.Offset = variableOffset;
				++variableOffset;
				variable.ResolveType(TopScope);
			}

			foreach (var rulebook in Rules.Rulebooks)
			{
				rulebook.ResultType = TopScope.FindType(rulebook.ResultTypeName);
				if (rulebook.ResultType == null) 
					throw new CompileError("Could not find type '" + rulebook.ResultTypeName + "'.");

				if (rulebook.ResultTypeName == "VOID") throw new CompileError("Rules cannot return void.");
				if (rulebook.ResultTypeName != "RULE-RESULT")
				{
					if (rulebook.DefaultValue == null)
						throw new CompileError("Rules that return values must have a default value specified using 'default of rule..'");
				}

				rulebook.ConsiderFunction = new Declaration();
				rulebook.ConsiderFunction.Terms = rulebook.DeclarationTerms;
				rulebook.ConsiderFunction.ReturnTypeName = rulebook.ResultTypeName;
				rulebook.ConsiderFunction.Body = new LambdaBlock(
					new Ast.Return(new Token())
					{
						Value = new StandardLibrary.ConsiderRuleBookFunctionNode(new Token(), rulebook)
					});
				rulebook.ConsiderFunction.OwnerContextID = ID;
				rulebook.ConsiderFunction.Type = DeclarationType.Test;
				PendingEmission.Add(rulebook.ConsiderFunction);
			}

			foreach (var macro in TopScope.Macros)
				macro.ResolveTypes(TopScope);

			foreach (var declaration in PendingEmission)
			{
				if (declaration.Type == DeclarationType.Macro)
					TopScope.Macros.Add(declaration);
				
				declaration.ResolveTypes(TopScope);
			}

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

			into.StringTable.Compress();

			if (Compile.Debug)
			{
				Compile.DebugWrite("\nCOMPILED CODE:\n");
				VirtualMachine.Debug.DumpOpcode(into.Data, Compile.DebugWrite, 1);
				Compile.DebugWrite(" STRING TABLE:\n");
				for (int i = 0; i < into.StringTable.PartTable.Length; ++i)
					Compile.DebugWrite(" " + i.ToString() + ": " + into.StringTable.PartTable[i] + "\n");
				Compile.DebugWrite(" STRING DATA:\n");
				Compile.DebugWrite("  " + into.StringTable.StringData + "\n");
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

		public int StaticVariableCount { get { return TopScope.Variables.Count; } }

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

			//Insertion-sort on token length
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
