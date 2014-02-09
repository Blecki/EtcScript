using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Node
	{
		public virtual void Emit(VirtualMachine.InstructionList into)
		{
			throw new NotImplementedException();
		}
	}
}
