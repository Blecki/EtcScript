using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class FunctionCall : Statement
	{
		public List<Node> Parameters = new List<Node>();
	}
}
