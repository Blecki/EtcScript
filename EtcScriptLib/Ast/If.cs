using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class If : Statement
	{
		public Node Header;
		public Block ThenBlock;
		public Block ElseBlock;
	}
}
