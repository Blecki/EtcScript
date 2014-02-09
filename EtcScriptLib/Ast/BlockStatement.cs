using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class BlockStatement : Statement
	{
		public List<Statement> Statements = new List<Statement>();

		public override void Emit(VirtualMachine.InstructionList into)
		{
			foreach (var statement in Statements)
				statement.Emit(into);
		}
	}
}
