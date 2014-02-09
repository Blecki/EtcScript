using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ForeachIn : Statement
	{
		public String VariableName;
		public Node Source;
		public Node Body;
	}
}
