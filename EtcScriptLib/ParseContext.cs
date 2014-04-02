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
		public struct Operator
		{
			public String token;
			public VirtualMachine.InstructionSet instruction;
		}

		public List<Control> Controls = new List<Control>();
		public ParseScope ActiveScope { get; private set; }

		public void PushNewScope()
		{
			var newScope = new ParseScope { Parent = ActiveScope };
			ActiveScope = newScope;
		}

		public void PopScope()
		{
			if (ActiveScope.Parent == null) throw new InvalidOperationException("Can't pop top scope");
			ActiveScope = ActiveScope.Parent;
		}

		public ParseContext()
		{
			ActiveScope = new ParseScope();
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
