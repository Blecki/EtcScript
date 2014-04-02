using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Statement : Node
	{
		public Statement(Token Source) : base(Source) { }

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new NotImplementedException();
		}
	}
}
