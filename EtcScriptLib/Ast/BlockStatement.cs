using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class BlockStatement : Statement
	{
		public List<Node> Statements = new List<Node>();
		private ParseScope LocalScope;

		public BlockStatement(Token Source) : base(Source) { }

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			foreach (var statement in Statements)
				statement.Emit(into, OperationDestination.Discard);
			if (LocalScope.Variables.Count > 0) into.AddInstructions("CLEANUP NEXT", LocalScope.Variables.Count);
		}

		public override string ToString()
		{
			return "{ \n" +
				String.Join(";\n", Statements.Select(s => s.ToString())) + " }";
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;
			LocalScope = Scope.Push(ScopeType.Block);
			Statements = new List<Node>(Statements.Select(s => s.Transform(LocalScope)).Where(n => n != null));
			return this;
		}


	}
}
