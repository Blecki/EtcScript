using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class BlockStatement : Statement
	{
		public List<Node> Statements = new List<Node>();

		public BlockStatement(Token Source) : base(Source) { }

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			foreach (var statement in Statements)
				statement.Emit(into, OperationDestination.Discard);
		}

		public override void Debug(int depth)
		{
			foreach (var statement in Statements)
				statement.Debug(depth + 1);
		}

		public override Node Transform(ParseScope Scope)
		{
			Statements = new List<Node>(Statements.Select(s => s.Transform(Scope)));
			return this;
		}
	}
}
