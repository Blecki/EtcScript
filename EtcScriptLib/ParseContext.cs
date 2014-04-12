using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum ControlBlockType
	{
		NoBlock,
		OptionalBlock,
		RequiredBlock
	}

	public class Control
	{
		public List<DeclarationTerm> DeclarationTerms;
		public Func<List<Ast.Node>, Ast.Node, Ast.Node> TransformationFunction;
		public ControlBlockType BlockType = ControlBlockType.RequiredBlock;

		public static Control Create(
			Declaration Declaration,
			ControlBlockType BlockType,
			Func<List<Ast.Node>, Ast.Node, Ast.Node> TransformationFunction)
		{
			return new Control
			{
				DeclarationTerms = Declaration.Terms,
				TransformationFunction = TransformationFunction,
				BlockType = BlockType
			};
		}
	}

	public class ParseContext
	{
		public RuleSet Rules = new RuleSet();
		public List<Declaration> PendingEmission = new List<Declaration>();
		public int ID = 0;

		public void EmitDeclarations()
		{
			var into = new VirtualMachine.InstructionList();

			foreach (var type in ActiveScope.Types)
				if (type.Origin == TypeOrigin.Script)
					foreach (var member in type.Members)
					{
						if (String.IsNullOrEmpty(member.DeclaredTypeName))
							member.DeclaredType = Type.Generic;
						else
						{
							member.DeclaredType = ActiveScope.FindType(member.DeclaredTypeName);
							if (member.DeclaredType == null) throw new CompileError("Could not find type '" +
								member.DeclaredTypeName + "'.");
						}
					}

			foreach (var declaration in PendingEmission)
			{
				if (!String.IsNullOrEmpty(declaration.ReturnTypeName))
				{
					declaration.ReturnType = declaration.DeclarationScope.FindType(declaration.ReturnTypeName);
					if (declaration.ReturnType == null) throw new CompileError("Could not find type '" +
						declaration.ReturnTypeName + "'.", declaration.Body.Body.Source);
				}
				else
					declaration.ReturnType = Type.Void;

				CreateParameterDescriptors(declaration.DeclarationScope, declaration.Terms);

				foreach (var term in declaration.Terms)
					if (term.Type == DeclarationTermType.Term)
					{
						if (String.IsNullOrEmpty(term.DeclaredTypeName))
							term.DeclaredType = Type.Generic;
						else
						{
							term.DeclaredType = declaration.DeclarationScope.FindType(term.DeclaredTypeName);
							if (term.DeclaredType == null) throw new CompileError("Could not find type '" +
								term.DeclaredTypeName + "'.");
						}
					}
			}				

			foreach (var declaration in PendingEmission)
			{
				declaration.OwnerContextID = ID;
				declaration.Body.EmitInstructions(declaration.DeclarationScope, into);
				if (declaration.WhenClause != null) declaration.WhenClause.EmitInstructions(declaration.DeclarationScope, into);

				if (Compile.Debug)
				{
					Compile.EmitDebugDump(declaration);
					Compile.DebugWrite("\n");
				}
			}

			foreach (var declaration in PendingEmission)
			{
				foreach (var point in declaration.Body.CallPoints)
					into[point] = declaration.Body.EntryPoint;
				if (declaration.WhenClause != null)
					foreach (var point in declaration.WhenClause.CallPoints)
						into[point] = declaration.WhenClause.EntryPoint;
			}

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

		// Given a list of terms, create the variable objects to represent the parameters of the declaration
		private static void CreateParameterDescriptors(ParseScope Scope, List<DeclarationTerm> Terms)
		{
			int parameterIndex = -3;
			for (int i = Terms.Count - 1; i >= 0; --i)
			{
				if (Terms[i].Type == DeclarationTermType.Term)
				{
					var declaredType = String.IsNullOrEmpty(Terms[i].DeclaredTypeName) ? Type.Generic : Scope.FindType(Terms[i].DeclaredTypeName);
					if (declaredType == null) throw new CompileError("Could not find type '" + Terms[i].DeclaredTypeName + "'.");
					var variable = new Variable
					{
						Name = Terms[i].Name,
						Offset = parameterIndex,
						DeclaredType = declaredType
					};					
					--parameterIndex;
					Scope.Variables.Add(variable);
				}
			}
		}

		public struct Operator
		{
			public String token;
			public VirtualMachine.InstructionSet instruction;
		}

		public List<Control> Controls = new List<Control>();
		public ParseScope ActiveScope { get; private set; }

		public void PushNewScope()
		{
			var newScope = new ParseScope { Parent = ActiveScope, EnvironmentContext = this };
			ActiveScope = newScope;
		}

		public void PopScope()
		{
			if (ActiveScope.Parent == null) throw new InvalidOperationException("Can't pop top scope");
			ActiveScope = ActiveScope.Parent;
		}

		public ParseContext()
		{
			ActiveScope = new ParseScope { EnvironmentContext = this };
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
