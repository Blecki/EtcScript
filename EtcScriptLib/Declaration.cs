using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class Declaration
	{
		public String Type;
		public String Name;
		public List<String> Arguments;
		public VirtualMachine.InstructionList Instructions;
		public Ast.Node Body;
	}
}
