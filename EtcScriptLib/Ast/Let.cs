using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Let : Statement
	{
		public Node LHS;
		public Node Value;
	}
}
