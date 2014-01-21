using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
	public class Declaration
	{
		public String Type;
		public String Name;
		public List<String> Arguments;
		public VirtualMachine.InstructionList Instructions;
	}
}
