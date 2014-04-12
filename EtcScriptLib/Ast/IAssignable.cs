using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public interface IAssignable
	{
		void EmitAssignment(VirtualMachine.InstructionList into);
	}
}
